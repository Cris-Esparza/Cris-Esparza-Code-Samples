using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UIElements;

public class Seal : MonoBehaviour
{
    private float turnDelay;
    private float hitSpeedMin = 1f;
    [SerializeField] private Animator animator;
    private AnimatorStateInfo animState;

    public bool launched;
    Vector3 startPosition;

    [SerializeField] ParticleSystem[] sealParticles;
    [SerializeField] GameObject deathParticlesObject;
    [SerializeField] GameObject moneyParticlesObject;

    Prop prop;
    BoxCollider2D boxColl;

    float landingDelay = 0f;
    bool spinStarted = false;

    // Start is called before the first frame update
    void Start()
    {
        turnDelay = 0f;
        launched = false;
        startPosition = transform.position;
        hitSpeedMin = MonoBehaviourSingletonPersistent<Constants>.Instance.minCollisionEnemyDeathSpeed;
        if(GetComponent<Prop>() != null)
            GetComponent<Prop>().hitSpeedMin = hitSpeedMin;

        prop = GetComponent<Prop>();
		prop.LaunchSpeed = MonoBehaviourSingletonPersistent<Constants>.Instance.sealFlipSpeed;
		boxColl = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(landingDelay);

        turnDelay -= Time.deltaTime;
        turnDelay = turnDelay < 0f ? 0f : turnDelay;

        
        if (!launched && turnDelay <= 0f)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            turnDelay = Random.Range(1f, 5f);
           
        }

        if (launched && (transform.position - startPosition).magnitude < 1f)
        {
            launched = false;
            animator.SetBool("isSwat", false);

			
            landingDelay = .01f;
        }  

        if(!prop.Launched)
        {
            DelayTimer();
        }

        if(!launched && landingDelay <= 0)
        {
            sealParticles[0].Stop();
            sealParticles[1].Stop();
            sealParticles[2].Stop();
           
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // don't do anything while spinning through the air
        if (!launched && landingDelay <= 0)
        {
            if (other.tag == "Bullet")
            {
                // play swat animator
                animator.SetBool("isSwat", true);
                AudioManager.Instance.Play(Sounds.SealSwat);

                other.GetComponentInParent<Rigidbody2D>().velocity *= -1;
            }

            if (other.tag == "Penguin")
            {
                if (other.GetComponent<Penguin>().invulnerableTimer <= 0)
                {
                    if (other.attachedRigidbody.velocity.magnitude > hitSpeedMin)
                    {
                        MonoBehaviourSingletonPersistent<ScoreManager>.Instance.IncreaseScore(MonoBehaviourSingletonPersistent<Constants>.Instance.sealPoints, TrickNames.Seal);
                        MonoBehaviourSingletonPersistent<ScoreManager>.Instance.SpawnFloatingPoints(MonoBehaviourSingletonPersistent<Constants>.Instance.sealPoints);
                        launched = true;

                        AudioManager.Instance.Play(Sounds.SealDeath);
                        AudioManager.Instance.Play(Sounds.PlayerSlideHit);
                        AudioManager.Instance.Play(Sounds.SealSpin);
                        spinStarted = true;

                        //Creates the deathParticles and plays them when seal is killed
                        Instantiate(deathParticlesObject, transform.position, Quaternion.identity);
                        sealParticles[0].Play();
                        sealParticles[1].Play();
                        sealParticles[2].Play();
                        

                        GameObject moneyParticles = Instantiate(moneyParticlesObject, startPosition, Quaternion.identity);

                        Destroy(moneyParticles, 1.5f);

                        boxColl.enabled = false;

                        DataTracker.Instance.numberOfSealHits++;
                    }
                    else
                    {
                        bool isPenguinRight = other.transform.position.x <= transform.position.x;
                        MonoBehaviourSingletonPersistent<ScoreManager>.Instance.ResetMultiplier();
                        MonoBehaviourSingletonPersistent<ScoreManager>.Instance.EndTrick();
                        other.GetComponent<Penguin>().OnKnockback(isPenguinRight);
                        animator.SetBool("isSwat", true);
                        AudioManager.Instance.Play(Sounds.SealSwat);

                        DataTracker.Instance.numberOfSealDamages++;
                    }
                }
            }

            if (other.tag == "Icicle")
            {
                //Awards points for killing seal
                MonoBehaviourSingletonPersistent<ScoreManager>.Instance.IncreaseScore(MonoBehaviourSingletonPersistent<Constants>.Instance.sealPoints, TrickNames.Seal);
                MonoBehaviourSingletonPersistent<ScoreManager>.Instance.SpawnFloatingPoints(MonoBehaviourSingletonPersistent<Constants>.Instance.sealPoints);
                launched = true;
            }
        }
    }

    /// <summary>
    /// Animator calls the end of swat script to reset it
    /// </summary>
    public void SetEndOfSwat()
    {
        animator.SetBool("isSwat", false);
    }

    private void DelayTimer()
    {
        if(landingDelay > 0)
        {
            landingDelay -= Time.deltaTime;
        }
        else
        {
            boxColl.enabled = true;
			if (spinStarted)
			{
				spinStarted = false;
				AudioManager.Instance.Play(Sounds.SealLand);
				AudioManager.Instance.Pause(Sounds.SealSpin);
			}
		}
    }
}
