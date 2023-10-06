using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// When triggered the collectible disappears and tells the UI to update
/// </summary>
public class Collectible : CollectibleSubject
{
    [Tooltip("This number determines which unique pearl this is (like a serial number)")]
    [SerializeField] public readonly int pearlNumber;

    //direction of oscillation
    [SerializeField] Vector3 movementVector = new Vector3(0f, 0f, 0f);

    //How long the period of the wave in oscillation should be
    [SerializeField] float period = 4f;

    //Open Shell sprite
    [SerializeField] private Sprite clamShellOpen;

    float movementFactor;
    Vector3 startingpos;
    public bool isCollected;

    //variables for swapping sprite
    [SerializeField] public GameObject shell;
    private SpriteRenderer shellSpriteRenderer;
    private SpriteRenderer spriteRenderer;
    private Animator collectibleAnimator;

    [SerializeField]
    public ParticleSystem blackHoleSparkle;

    void Start()
    {
        //starting position of hover
        startingpos = transform.position;

        //if no sprite is displaying, display sprite one
        shellSpriteRenderer = shell.GetComponent<SpriteRenderer>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (shellSpriteRenderer.sprite == null)
        {
            shellSpriteRenderer.sprite = clamShellOpen;
        }

        //saving constant variable
        period = MonoBehaviourSingletonPersistent<Constants>.Instance.period;

        collectibleAnimator = GetComponent<Animator>();

        isCollected = false;
    }

    private void OnBecameInvisible()
    {
        if(isCollected)
        {
            NotifyObservers();
            isCollected = false;
            Destroy(gameObject);
        }
    }

    void Update()
    {
        //Using period variable, determine cycle time
        if (period <= 0f) { return; }
        float cycles = Time.time / period;

        //Oscillates sprite up and down using sine wave
        const float tau = Mathf.PI * 2;
        float rawSineWave = Mathf.Sin(cycles * tau);

        //range zero to one
        movementFactor = rawSineWave / 2f + 0.5f;

        //updating movement of object
        Vector3 offset = movementVector * movementFactor;
        transform.position = startingpos + offset;
    }

    public void SetAnimator()
    {
        collectibleAnimator = GetComponent<Animator>();
    }
}
