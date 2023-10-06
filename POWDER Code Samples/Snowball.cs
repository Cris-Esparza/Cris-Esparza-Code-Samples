using Powder.Singleton;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Snowball : MonoBehaviour
{
    [System.Serializable]
    public struct Stats
    {
        [Tooltip("Speed that player starts and resets to")]
        public float startSpeed;

        [Tooltip("Speed at which the player moves forward")]
        public float speed;

        [Tooltip("Force of movement left and right")]
        public float horizontalForce;

        [Tooltip("Force applied for jump")]
        public float jumpForce;

        [Tooltip("Acceleration rate of forward movment")]
        public float acceleration;

        [Tooltip("Speed of forward rotation of snowball")]
        public float rotationSpeed;

        [Tooltip("Nubmer of lives for all snowballs")]
        public int lives;

        [Tooltip("Rate at which the player grows in size")]
        public Vector3 objectScale;
    }

    public Stats playerStats;

    // Audio sources for player sounds
    public AudioSource movementSound, jumpSound, landSound;
    // Particle effects game objects
    public GameObject snowKick;
    // Rigidbody of player
    public Rigidbody rigidbody;
    // Player and snowball game objects
    public GameObject snowball;
    // Size of snowball in game
    public Vector3 curSize, newSize, resetSize, curPos;
    public bool isDestroyed;
    public Player player;
    public float startingForce = 10f;

    private bool isGrounded = true;
    private bool canPlayLandingSound;
    private Renderer rend;

    void Awake()
    {
        rend = snowball.GetComponent<Renderer>();
        playerStats.objectScale = new Vector3(0.003f, 0.003f, 0.003f);
        rigidbody = GetComponent<Rigidbody>();
        transform.localScale = newSize;
    }

    void Start()
    {
        // set state of particle systems
        snowKick.SetActive(true);
        isDestroyed = false;
    }

    void FixedUpdate()
    {
        // Input variables
        float horizontal = Input.GetAxisRaw("Horizontal");
        float jumping = Input.GetAxisRaw("Jump");

        // Jump force dependent on speed
        playerStats.jumpForce = startingForce * playerStats.speed;

        // Snowballs size in game
        curSize = transform.localScale;

        // getting current position of snowball
        curPos = CurrentPosition();

        // Speed increase
        playerStats.speed = playerStats.speed + (playerStats.acceleration * Time.deltaTime);

        // Increase in size over time
        transform.localScale += playerStats.objectScale;

        // Forward movement
        MoveForward(playerStats.speed, playerStats.rotationSpeed);

        // Movement
        if (horizontal < 0)
        {
            MoveLeft(playerStats.horizontalForce, playerStats.rotationSpeed);
        }
        else if (horizontal > 0)
        {
            MoveRight(playerStats.horizontalForce, playerStats.rotationSpeed);
        }
        if (jumping != 0 && isGrounded)
        {
            Jump(playerStats.jumpForce);
        }

        // Particle system for jumping
        if (!isGrounded)
        {
            snowKick.SetActive(false);      // deactivate snow kickup particle effect
        }
        snowKick.SetActive(true);

        // if snowball has been destroyed
        if (isDestroyed)
        {
            player.RemoveSnowball(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // checking if player is on the ground
        if (collision.gameObject.tag == "Ground")
        {
            isGrounded = true;
            canPlayLandingSound = true;
            if (!landSound.isPlaying && canPlayLandingSound == true)
            {
                canPlayLandingSound = false;
                landSound.Play();
            }
            movementSound.UnPause();     // resume movement sfx
        }
        else
        {
            movementSound.Pause();       // pause movement sfx
        }

        // checking if player collides with obstacle
        if (collision.gameObject.tag == "Obstacle")
        {
            isDestroyed = DestroySnowball();   
        }
    }

    void MoveForward(float speed, float rotSpeed)
    {
        transform.Translate(Vector3.forward * Time.deltaTime * speed, Space.World);
        snowball.transform.Rotate(Vector3.right, rotSpeed);
    }

    void MoveLeft(float force, float rotSpeed)
    {
        rigidbody.AddForce(Vector3.left * force * Time.deltaTime, ForceMode.Impulse);
        snowball.transform.Rotate(Vector3.forward, rotSpeed);
    }

    void MoveRight(float force, float rotSpeed)
    {
        rigidbody.AddForce(Vector3.right * force * Time.deltaTime, ForceMode.Impulse);
        snowball.transform.Rotate(Vector3.back, rotSpeed);
    }

    void Jump(float force)
    {
        rigidbody.AddForce(Vector3.up * force, ForceMode.Impulse);
        isGrounded = false;
        jumpSound.Play();
    }

    bool DestroySnowball()
    {
        isDestroyed = true;
        snowKick.SetActive(false);      // deactivate particle effects
        movementSound.Stop();        // stop playing movement sfx
        //rend.enabled = false;       // disable mesh renderer or snowball
        return isDestroyed;
    }

    //void ResetSize()
    //{
    //    transform.Translate(new Vector3(0,3,0), Space.World);
    //    transform.localScale = newSize;
    //    playerStats.speed = playerStats.startSpeed;     // resets players speed
    //}

    Vector3 CurrentPosition()
    {
        curPos = transform.position;
        return curPos;
    }
}
