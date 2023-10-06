using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Prop : MonoBehaviour
{

    [SerializeField] private float rotationSpeed = 1500f;
    [SerializeField] private float launchSpeed = 10f;
    [SerializeField] private float gravityScale = 1f;
    public float hitSpeedMin;

    bool launched;
    public bool Launched { get { return launched; }
    }
    Vector3 startPosition;
    Rigidbody2D rb;

    [SerializeField] private float landingDelay = 0f;

    public float LaunchSpeed
    {
        get { return launchSpeed; }
        set { launchSpeed = value; }
    }

    private void Start()
    {
        launched = false;
        hitSpeedMin = MonoBehaviourSingletonPersistent<Constants>.Instance.hitSpeedMinimum;
        startPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Reset when we land back on the ground
        if (launched && rb.velocity.y < 0 && (transform.position - startPosition).magnitude < 1f)
        {
            rb.gravityScale = 0f;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = 0f;
            transform.rotation = Quaternion.identity;
            transform.position = startPosition;
            launched = false;

            landingDelay = .01f;
        }

        if(!launched)
        {
            DelayTimer();
        }
    }

    /// <summary>
    /// Flip props when the player hits them
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // if player hits us
        if (!launched && collision.tag == "Penguin" && landingDelay <= 0)
        {
            Rigidbody2D prb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (prb.velocity.magnitude > hitSpeedMin && collision.GetComponent<Penguin>().invulnerableTimer <= 0)
            {
                // shoot into air depending on speed hit
                rb.velocity = Vector2.up * launchSpeed;
                // spin around
                rb.angularVelocity = prb.velocity.x < 0 ? -rotationSpeed : rotationSpeed;
                // fall
                rb.gravityScale = gravityScale;
                //Checks to see if the prop script is attached to a seal so as to not give double points
                if (gameObject.tag != "Seal")
                {
                    MonoBehaviourSingletonPersistent<ScoreManager>.Instance.IncreaseScore(MonoBehaviourSingletonPersistent<Constants>.Instance.propPoints, TrickNames.Prop);
                    MonoBehaviourSingletonPersistent<ScoreManager>.Instance.SpawnFloatingPoints(MonoBehaviourSingletonPersistent<Constants>.Instance.propPoints);
                }
                launched = true;
            }
        }
    }
    
    public void DoFlip()
    {
        // shoot into air depending on speed hit
        rb.velocity = Vector2.up * launchSpeed;
        // spin around
        rb.angularVelocity = rotationSpeed;
        // fall
        rb.gravityScale = gravityScale;

        launched = true;
    }

    private void DelayTimer()
    {
        if (landingDelay > 0)
        {
            landingDelay -= Time.deltaTime;
        }
    }
}
