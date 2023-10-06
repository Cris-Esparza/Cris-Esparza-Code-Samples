using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Icicle : MonoBehaviour
{
    private Rigidbody2D rb2d;

    [SerializeField]
    public GameObject bigIcicle;

    [SerializeField]
    public GameObject breakParticles;

    private bool isDead;    //Handles the icicle dying

    //handles the respawning of big icicle
    public bool respawn;
    private float respawnTime;
    private GameObject babyIcicle;
    private Vector3 icicleScaler;
    private float currentScale = 0.1f;

    [SerializeField]
    private bool isGrav = false;    //Handles whether the icicle is gravity based

    void Start()
    {
        isDead = false;
        rb2d = GetComponent<Rigidbody2D>(); // save for efficiency
        icicleScaler = new Vector3(currentScale, currentScale, currentScale);
    }

    private void Update()
    {
        //Checks to see if the icicle is an upside down icicle
        if (isGrav)
        {
            //Checks to see if the respawn of the big icicle was triggered
            if (respawn)
            {
                BigIcicleRespawnCooldown();
            }
            else if (currentScale <= 1f && babyIcicle != null)
            {
                //Scales big icicle up to give a growing effect
                currentScale += .01f;
                icicleScaler = new Vector3(currentScale, currentScale, currentScale);
                babyIcicle.transform.localScale = icicleScaler;
            }
            else if (currentScale >= 1f && babyIcicle != null)
            {
                //Once icicle is fully grown, reset variables and allow big icicle to fall
                currentScale = 1f;
                icicleScaler = new Vector3(currentScale, currentScale, currentScale);
                babyIcicle.transform.localScale = icicleScaler;
                babyIcicle.GetComponent<BigIcicle>().isGrav = true;
                currentScale = 0.1f;
                icicleScaler = new Vector3(currentScale, currentScale, currentScale);
                babyIcicle = null;
            }
        }
    }

    /// <summary>
    /// Updates big icicle respawn time and respawns the big icicle
    /// </summary>
    private void BigIcicleRespawnCooldown()
    {
        respawnTime += Time.deltaTime;
        if (respawnTime >= MonoBehaviourSingletonPersistent<Constants>.Instance.bigIcicleRespawnTime)
        {
            //Instantiates the big icicle
            respawn = false;
            respawnTime = 0f;
            babyIcicle = Instantiate(bigIcicle, transform.position, Quaternion.Euler(new Vector3(0,0,180f)), transform.GetChild(0).transform);
            babyIcicle.GetComponent<BigIcicle>().isGrav = false;
            babyIcicle.transform.localScale = icicleScaler;
        }
    }
    /// <summary>
    /// Kills enemy if touched at too high of a velocity
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckPenguinSpeed(collision);
    }

    /// <summary>
    /// On became invisible when dead, destroy the object
    /// </summary>
    private void OnBecameInvisible()
    {
        if(isDead)
        {
            Destroy(gameObject);
        }
    }

    public void CheckPenguinSpeed(Collider2D collision)
    {
        if (!isDead)
        {
            // Make sure its the penguin, and he's sliding
            if (collision.gameObject.GetComponent<Rigidbody2D>() != null &&
               collision.gameObject.tag == "Penguin")
            {
                Vector2 incomingVel = collision.gameObject.GetComponent<Rigidbody2D>().velocity;
                // Debug.Log("Enemy Hit: " + collision.relativeVelocity.magnitude);

                //Checks if penguin is going fast enough and checks if the penguin has an angle match before destroying icicle.
                if (incomingVel.magnitude > MonoBehaviourSingletonPersistent<Constants>.Instance.minCollisionIcicleBreakSpeed)
                {
                    isDead = true;
                    Instantiate(breakParticles, collision.gameObject.transform.position, transform.rotation);
                    if (!collision.gameObject.GetComponent<Penguin>().PlayerStateData.hitHazard)
                    {
                        MonoBehaviourSingletonPersistent<ScoreManager>.Instance.IncreaseScore(MonoBehaviourSingletonPersistent<Constants>.Instance.iciclePoints, TrickNames.Icicle);
                        MonoBehaviourSingletonPersistent<ScoreManager>.Instance.SpawnFloatingPoints(MonoBehaviourSingletonPersistent<Constants>.Instance.iciclePoints);
                    }
                    Destroy(gameObject);

                    // Icicle break sound
                    AudioManager.Instance.Play(Sounds.IcicleBreak);
                    if (Random.Range(0, 10) == 0) AudioManager.Instance.Play(Sounds.SquawkSlideHit);

                    DataTracker.Instance.numberOfIcicleHits++;
                }
                //Prevents the object from moving on collision with the penguin
                else if (!collision.gameObject.GetComponent<Penguin>().PlayerStateData.hitHazard)
                {
                    // Damage penguin and knockback based on direction of impact
                    MonoBehaviourSingletonPersistent<ScoreManager>.Instance.ResetMultiplier();
                    MonoBehaviourSingletonPersistent<ScoreManager>.Instance.EndTrick();
                    collision.gameObject.GetComponent<Penguin>().OnKnockback(incomingVel.x < 0);

                    DataTracker.Instance.numberOfIcicleDamages++;
                }
            }
        }
    }
}
