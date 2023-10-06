using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Narwhal : NarwhalSubject
{
    // saved for efficiency
    float moveImpulse;  //movement speed
    Rigidbody2D narwhalRb2d;    //Store rb2d

    private Transform[] points;     //The points the narwhal follows
    public void SetPoints(Transform[] transforms)   //Set for the points
    {
        points = transforms;
    }
    private int pointsCompleted;    //Tracks how many points the narwhal has moved through
    private float tParam;       //Tparam used to smooth route
    private Vector2 objectPosition;     //Set the position for the narwhal
    private bool coroutineAllowed;      //Determines if the coroutine is allowed
    private bool isDead;       //Determines if the narwhal is dead
    private float deathTimer;   //death timer for the narhwal
    private float currentTimer;     //current time left

    private bool isVisible = false;
    private bool flipped;

    private NarwhalSpawner _spawner;    //Store the narwhal spawner
    public void SetSpawner(NarwhalSpawner spawner)      //Set the spawner so that it can notify it 
    {
        _spawner = spawner;
        Attach(spawner);
    }
    public void SetSpeed(float speed)
    {
        moveImpulse = speed;
    }

    // Start is called before the first frame update
    void Start()
    {
        // save for efficiency
        narwhalRb2d = GetComponent<Rigidbody2D>();
        deathTimer = MonoBehaviourSingletonPersistent<Constants>.Instance.deathTimer;

        //Set vars for the movement coroutine
        pointsCompleted = 0;
        tParam = 0f;
        coroutineAllowed = true;
        flipped = false;
    }

    // Update is called once per frame
    void Update()
    {   
        //if the coroutine can be performed, causes the narwhal to follow the route
        if (coroutineAllowed)
        {
            StartCoroutine(FollowRoute(pointsCompleted));
        }
        //IF the narwhal is dead, timer starts before killing the gameobject
        if(isDead)
        {
            currentTimer -= Time.deltaTime;
            if(currentTimer <= 0)
            {
                Destroy(gameObject);
            }
        }

		// check if narwhal has entered the screen space
		Vector3 relativePos = Camera.main.WorldToViewportPoint(transform.position);
		if ((relativePos.x > 0 && relativePos.x < 1) && (relativePos.y > 0 && relativePos.y < 1))
		{
            if (!isVisible)
            {
                AudioManager.Instance.Play(Sounds.NarwhalRoar);
            }
			isVisible = true;
		}
		else
		{
			isVisible = false;
		}
	}

    /// <summary>
    /// Narwhal follows the route
    /// </summary>
    /// <param name="routeNum"></param>
    /// <returns></returns>
    private IEnumerator FollowRoute(int routeNum)
    {
        //Turn off coroutine
        coroutineAllowed = false;

        //Store the points
        Vector2 p0 = points[0].position;
        Vector2 p1 = points[1].position;
        Vector2 p2 = points[2].position;
        Vector2 p3 = points[3].position;

        //Moves the narhwal down the route
        while (tParam < 1 && !isDead)
        {
            tParam += Time.deltaTime * (moveImpulse / Vector2.Distance(p0, p3));
            objectPosition = Mathf.Pow(1 - tParam, 3) * p0 + 3 * Mathf.Pow(1 - tParam, 2) * tParam * p1 + 3 * (1 - tParam) * Mathf.Pow(tParam, 2) * p2 + Mathf.Pow(tParam, 3) * p3;
            transform.up = new Vector3(objectPosition.x, objectPosition.y, 0f) - transform.position;
            if (points[0].position.x > points[3].position.x && flipped == false)
            {
                transform.localScale = new Vector3(-1, 1, 1);
                flipped = true;
            }
            else if (flipped == false)
            {
                transform.localScale = new Vector3(1, 1, 1);
                flipped = true;
            }
            transform.position = objectPosition;

            yield return new WaitForEndOfFrame();
        }

        //Reset the narwhal
        tParam = 0f;
        pointsCompleted += 1;
        flipped = false;

        if (pointsCompleted > points.Length - 1)
        {
            pointsCompleted = 0;
        }
        if (!isDead)
        {
            Destroy(gameObject);
        }

    }

    /// <summary>
    /// Damage the penguin if it touches the top or bottom of the penguin.
    /// </summary>
    /// <param name="collision"></param>
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Penguin") && collision.gameObject.GetComponent<Penguin>().invulnerableTimer <= 0)
        {
            //Change player score
			Penguin penguinComp = collision.gameObject.GetComponent<Penguin>();
			if (penguinComp == null) return;
			penguinComp.OnKnockback(narwhalRb2d.velocity.x < 0);
			MonoBehaviourSingletonPersistent<ScoreManager>.Instance.ResetScore();
            MonoBehaviourSingletonPersistent<ScoreManager>.Instance.ResetMultiplier();
            MonoBehaviourSingletonPersistent<ScoreManager>.Instance.EndTrick();

            DataTracker.Instance.numberOfNarwhalDamages++;
        }
    }

    //When the narwhal is destroyed, remove this from the list
    private void OnDestroy()
    {
        NotifyObservers();
        Detach(_spawner);
    }

    private void OnBecameVisible()
    {
        AudioManager.Instance.Play(Sounds.NarwhalRoar);
    }

    
}

