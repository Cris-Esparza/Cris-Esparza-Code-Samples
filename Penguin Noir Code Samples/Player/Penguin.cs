using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.InputSystem;

/// <summary>
/// Vars to store data about current player state
/// </summary>
public struct PlayerStateData
{
    // Env vars
    public bool facingRight; // whether the player is facing right or left
    public float horizVel; // current horizontal speed of player // HACK: this should probably be gotten through a reference to the player object
    public bool isGrounded; // whether the player is standing on ground
    public Vector2 lastGroundNormal; // saves the normal of the last ground contact
    public bool isStunnedFromKnockback;
    public bool hitBouncePad;
    public bool isSliding;
    public bool inWater; // whether the player is in water
    public bool hitWall;
    public bool hitHazard;
    public float currentSlideCooldown; // the amount of time before the player can slide again
    public float currentRespawnTime; //the amount of time before the player respawns
	public bool hasFinishedLevel;
	public bool isStomping;         //controls whether the penguin is stomping
	

	// Input vars
	public float horizInput;    // Input on the horizontal axis
	public float vertInput;     // Input on the vertical / jump axis
	public bool slideInput;     // if the player pressed the slide key
	public float playerYVel;    // If the player is rising
	public bool gunRisingInput; // Checks if player shoots gun to force the penguin upwards

	// Meta vars
	public PlayerStateType stateType; // type of player state
}

/// <summary>
/// Luis Martinez, Brendan Gould, Nathan Peckham
/// Lmarti32@uccs.edu, bgould2@uccs.edu, npeckham@uccs.edu
/// This class tracks the movement of the penguin
/// </summary>
public class Penguin : PlayerSubjectObserver
{

    // saved for efficiency
    Rigidbody2D penguinRb2d;
    Transform penguinTransform;
	PlayerCollectible pc;
    [SerializeField]
    private Animator penguinAnimator;
	[SerializeField] ParticleSystem[] penguinParticleSystem;

	//private GameObject snowEffect;
	[SerializeField] GameObject DamageParticle;
	private GameObject damagedEffect;

	public Rigidbody2D PenguinRb2d
    {
        get { return penguinRb2d; }
    }

	[SerializeField]
	private List<SpriteRenderer> penguinSprites;

	[SerializeField]
	private float currentWalkAngle = 0f;
	public float CurrentWalkAngle
	{
		get {
			return currentWalkAngle;
		}
	}

	private Vector2 lastBounceNorm;

	// Saves the direction vector of the swimming tube that was last touched
	Vector2 swimDirection = Vector3.right;
	Vector2 tubeCenter;
	Vector2 tubeCenterDirection;
	float swimAngle;
	float swimSign;

	public float invulnerableTimer = 0f;

	// State vars
	private PlayerStateData playerStateData; // data about current player state
	public PlayerStateData PlayerStateData
	{
		get {
			return playerStateData;
		}
	}

	private PlayerState state;

	public PlayerStateType State
	{
		get {
			return playerStateData.stateType;
		}
	}

	private PlayerStateManager stateManager;

	public bool Grounded
	{
		get {
			return playerStateData.isGrounded;
		}
	}

	Vector2 lastVelocity;

	private bool isGunRising = false; // Determines if the gun has caused the penguin to rise
	public bool IsGunRising           // Set statement for is gun rising
	{
		set {
			isGunRising = value;
		}
	}

	//Creates the varible for when you reach end of level
	public bool hasFinishedLevel;
	public bool HasFinishedLevel
	{
		get { return hasFinishedLevel; }
	}

	//Creates get/set for isStomping from player data
	public bool isStomping;
	public bool IsStomping
    {
		get { return isStomping; }
    }

	//Creates bool for whether the player can move yet
	public bool levelStartCooldown;
	public bool LevelStartCooldown
    {
		get { return levelStartCooldown; }
    }

	bool isGunEnabled = false;
	bool speedSoundPlayed = false;

    private bool diveParticleCheck;

    private float flipperParticleCD = 0;

    public bool isUnableToCollectPearls = false;
	private float pearlTimeMax = 1f;
	private float pearlTimeCurr = 0f;

	bool diveInput;
	bool shootInput;
	bool squawkInput;

    // Start is called before the first frame update
    void Start()
	{
		gameObject.transform.position += new Vector3(0, 0.2f, 0);
		// save for efficiency
		penguinTransform = GetComponent<Transform>();
		penguinRb2d = GetComponent<Rigidbody2D>();
        penguinParticleSystem = GetComponentsInChildren<ParticleSystem>();
		pc = GameObject.FindGameObjectWithTag("CollectibleUI").GetComponent<PlayerCollectible>();
		// Debug.LogError("Penguin::Start - pc == "  + pc);

		// grab every child sprite renderer
		foreach (Transform child in penguinTransform)
		{
			if (!child.gameObject.CompareTag("Gun"))
			{
				penguinSprites.Add(child.GetComponent<SpriteRenderer>());
			}
		}

		hasFinishedLevel = false;

        diveParticleCheck = false;


        // initialize state
        stateManager = new PlayerStateManager(this);
		// if the active scene is a cutscene initialize the player in the cutscene state
		if (MonoBehaviourSingletonPersistent<GameManager>.Instance.ActiveScene.name == "Test_CutScene")
		{
			state = new PlayerCutsceneState(this);
			playerStateData.stateType = PlayerStateType.Cutscene;
		}
		else // initialize the player as normal
		{
			state = new PlayerStandingState(this);
		}
		playerStateData.facingRight = true;

		GameObject penguinRig = transform.Find("penguinRig").gameObject;
		Vector3 temp = penguinRig.transform.localScale;
		temp.x = 1;
		penguinRig.transform.localScale = temp;

		isStomping = false;

		levelStartCooldown = true;

		DialogManager.isActive = false;

		//snowEffect = Instantiate(snowFall, new Vector3(this.transform.position.x, this.transform.position.y + 120), Quaternion.identity);

        //NotifyHealthObservers();
    }
		

    // Update is called once per frame
    void Update()
    {
		if (!levelStartCooldown)
		{
		// Update player state data
		CheckInput();
		UpdateGrounded();
		UpdateGroundAngle();
		UpdateSlideParticles();
		//UpdateSnowEffect();
		InvulnTimer();
		PearlTimer();

        playerStateData.horizVel = penguinRb2d.velocity.x;
		// Debug.Log("Grounded: " + isGrounded);
			if (ControllerInput())
            {
				shootInput = Gamepad.current.rightTrigger.wasPressedThisFrame;
				diveInput = Gamepad.current.leftTrigger.IsActuated();
				squawkInput = Gamepad.current.buttonWest.wasPressedThisFrame;
            }
            else
            {
				shootInput = Mouse.current.leftButton.wasPressedThisFrame;
				diveInput = Keyboard.current.sKey.isPressed;
				squawkInput = Keyboard.current.fKey.wasPressedThisFrame;
            }
		}

		// squawk button (very important)
		if (squawkInput)
		{
			AudioManager.Instance.Play(Sounds.SquawkMisc);
		}

		// safety catch if pc is not set
		if (pc == null)
		{
			pc = GameObject.FindGameObjectWithTag("CollectibleUI").GetComponent<PlayerCollectible>();
		}

		if (!isGunEnabled && !levelStartCooldown)
		{
			if (shootInput)
			{
				return;
			}
			isGunEnabled = true;
			GetComponent<GunController>().ActiveGun.StartDelay = false;
		}
		//Debug.Log("Y VELO: " + playerStateData.playerYVel);

		//Checks if the player is divebombing for particles
		if( playerStateData.playerYVel < -45f)
		{
			if (!penguinParticleSystem[7].isEmitting)
			{
				penguinParticleSystem[7].Play();
			}
		}
		else
		{
            penguinParticleSystem[7].Clear();
        }
		
	}

	/// <summary>
	/// FixedUpdate is called 50 times a second
	/// </summary>
	void FixedUpdate()
	{
		Physics2D.queriesStartInColliders = false;
		Vector2 up = transform.TransformDirection(Vector2.up) * 10;
		RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, 17);
		//Debug.DrawRay(transform.position, up, Color.red);

		if (hit.collider != null)
		{
			if (hit.collider.CompareTag("BigIcicle"))
			{
				hit.collider.gameObject.GetComponent<BigIcicle>().Fall();
			}
		}
		if (!levelStartCooldown)
        {
			// Calculate next player state
			PlayerState newState = stateManager.GetNextState(this, playerStateData);

			// Update player state
			if (newState != state)
			{
				state.TransitionFrom();
				state = newState;
				playerStateData.stateType = state.GetStateType();
				state.TransitionTo();
				//Debug.Log(newState);
			}

			state.Update();
		}
	}

	/// <summary>
	/// Orients the penguin upwards using the physics system
	/// <returns> Whether or not rotation is zeroed</returns>
	/// </summary>
	public void OrientUpwards()
	{
		if (PlayerStateData.facingRight) // Rotates penguin depending on what side he's facing
		{
			RotatePlayer(0);
		}
		else
		{
			RotatePlayer(-0);
		}
	}

	/// <summary>
	/// places the penguin flat (tangent) to the floor when sliding
	/// </summary>
	/// <returns>true if the penguin does not need to rotate using the physics system</returns>
	public void OrientFlat()
	{
		float rotation;
		// Get contacts
		List<ContactPoint2D> contacts = new List<ContactPoint2D>();
		penguinRb2d.GetContacts(contacts);
		// Get ground points
		contacts = contacts.FindAll(contact => contact.collider.gameObject.CompareTag("Platform"));
		// if no ground contacts exist then return
		if (contacts.Count >= 1)
		{
			// Define a ground point
			ContactPoint2D ground = contacts[0];
			// Calculate rotation based off of platform normal
			rotation = Mathf.Atan2(ground.normal.y, ground.normal.x) * Mathf.Rad2Deg - 90f;
			// Debug.Log(currentWalkAngle);

			if (Mathf.Abs(transform.rotation.eulerAngles.z - rotation) < 90)
			{
				// small change -> can do smoothing
				RotatePlayer(rotation);
			}
			else
			{
				// large change -> snap to angle
				penguinRb2d.MoveRotation(rotation);
			}
		}
	}

	/// <summary>
	/// Applies a movement force to the penguin if requested by input
	/// </summary>
	public void CheckForGroundMove()
	{

		// calculate walk impulse
		float moveImpulse = playerStateData.horizInput *
		                    MonoBehaviourSingletonPersistent<Constants>.Instance.walkImpulseMag *
		                    penguinRb2d.mass; // Using impulse, don't need to normalize by time
		                                      // Debug.Log("Pre-cap walk force is " + walkImpulse);
		float currentVel = penguinRb2d.velocity.x;

		// if the penguin is changing direction, change the walk impulse
		if (Mathf.Sign(playerStateData.horizInput) != Mathf.Sign(currentVel) && playerStateData.isGrounded &&
		    penguinRb2d.velocity.x != 0)
		{
			// Debug.Log("Turnaround");
			moveImpulse *= MonoBehaviourSingletonPersistent<Constants>.Instance.impulseTurnaroundMag;
		}
		// Set is sliding based on current velocity
		if (Mathf.Abs(currentVel) > MonoBehaviourSingletonPersistent<Constants>.Instance.maxWalkSpeed)
		{
			playerStateData.isSliding = true; // only stop if transitioning to !movingState
			penguinRb2d.sharedMaterial.friction =
				MonoBehaviourSingletonPersistent<Constants>.Instance.slideFriction; // set proper friction

			// Play sliding sound, update volume based on speed (faster is louder)
			AudioManager.Instance.Play(Sounds.PlayerSlide);
			AudioManager.Instance.SetVolume(Sounds.PlayerSlide,
			                                Mathf.Abs(currentVel) /
			                                    MonoBehaviourSingletonPersistent<Constants>.Instance.maxSlideSpeed);
		}

		// Check if max speed already reached
		if (Mathf.Abs(currentVel) > MonoBehaviourSingletonPersistent<Constants>.Instance.maxSlideSpeed)
		{
			// Debug.Log("maxVel");
			moveImpulse = 0;
		}
		else if (Mathf.Abs(currentVel) + Mathf.Abs(moveImpulse) >
		         MonoBehaviourSingletonPersistent<Constants>.Instance.maxSlideSpeed)
		{
			moveImpulse = Mathf.Sign(moveImpulse) *
			              (MonoBehaviourSingletonPersistent<Constants>.Instance.maxSlideSpeed - Mathf.Abs(currentVel));
		}

		// adds the impulse to the actual penguin
		// Debug.Log("walk force is " + walkImpulse);
		penguinRb2d.AddForce(new Vector2(moveImpulse, 0), ForceMode2D.Impulse);
		// Debug.Log("velocity is: " + penguinRb2d.velocity.x);
		// Debug.Log("MaxWalkSpeed" + (currentWalkVel == maxWalkSpeed));

		// this might just be a bandaid fix; stops maintained speed when landing if you turn around in the air and don't
		// hit a movement key
		if (Grounded)
		{
			if (currentVel > 0)
			{
				penguinRb2d.AddForce(new Vector2(-0.1f, 0), ForceMode2D.Impulse);
			}
			else if (currentVel < 0)
			{
				penguinRb2d.AddForce(new Vector2(0.1f, 0), ForceMode2D.Impulse);
			}
		}
	}

	/// <summary>
	/// Faces the sprite in the direction they are accellerating
	/// </summary>
	public void SetFacingDir()
	{
		if (PlayerStateData.horizVel < -MonoBehaviourSingletonPersistent<Constants>.Instance.minumumTurnVelocity && Mathf.Abs(PlayerStateData.horizVel) >= (Mathf.Abs(playerStateData.playerYVel) - .5f))
		{
			// facing left, only force left facing when actively moving left
			Vector3 temp = transform.localScale;
			temp.x = 1;
			transform.localScale = temp;
			// playerStateData.facingRight = false;
			playerStateData.facingRight = true;
		}
		else if (PlayerStateData.horizVel > MonoBehaviourSingletonPersistent<Constants>.Instance.minumumTurnVelocity && (Mathf.Abs(PlayerStateData.horizVel) >= Mathf.Abs(playerStateData.playerYVel) - .5f))
		{
			// facing right, only force right facing when actively moving right
			Vector3 temp = transform.localScale;
			temp.x = -1;
			transform.localScale = temp;
			// playerStateData.facingRight = true;
			playerStateData.facingRight = false;
		}
	}

	/// <summary>
	/// Faces the player to make the belly bounce off of the wall
	/// </summary>
	public void UpdateBounceDir(Vector2 normal)
	{
		lastBounceNorm = normal;
		// Calculate angle to rotate to
		float ang = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg;
		ang -= 90;
        // Rotate to desired angle
        penguinRb2d.MoveRotation(ang);
    }

	private void CheckInput()
	{

		bool controller = ControllerInput();
		// Update horizontal input
		//float horizontalRightInput =  Input.GetAxisRaw("Right");
		//float horizontalLeftInput = Input.GetAxisRaw("Left");
		//playerStateData.horizInput = controller ? Input.GetAxisRaw("gp_Horizontal"): horizontalRightInput - horizontalLeftInput;
		if (controller)
			playerStateData.horizInput = Gamepad.current.leftStick.x.ReadValue();
		else
			playerStateData.horizInput = (Keyboard.current.dKey.ReadValue() - Keyboard.current.aKey.ReadValue());

		// Update vertical input
		playerStateData.vertInput = 0;
		//playerStateData.vertInput -= Input.GetAxisRaw("Dive") + Input.GetAxisRaw("gp_Dive") > 0.3f ? 1 : 0;
		if (diveInput)
			playerStateData.vertInput = -1;

        // Update rising data
        playerStateData.playerYVel = penguinRb2d.velocity.y;

		// Update gun rising data
		playerStateData.gunRisingInput = isGunRising;

		
	}

	private static bool prevInputMethod = false;
	private static readonly string[] controllerButtons = { "gp_Horizontal", "gp_AimHorizontal", "gp_AimVertical", "gp_Shoot", "gp_Dive", "gp_Jump" };
	private static readonly string[] KBMButtons = { "Left", "Right", "ShootMain", "Jump", "Mouse X", "Mouse Y"};

	/// <summary>
	/// Checks to see if the most recent input is a controller
	/// </summary>
	/// <returns>true if the input came from a controller, false if kbm</returns>
	public static bool ControllerInput()
    {
		bool ret = prevInputMethod;
		// if any controller buttons are pressed, tell everyone we are using the controller
		if (GetAxis(controllerButtons))
			ret = true;
		// if any KBM buttons are pressed, tell everyone we are using the KBM
		if (GetAxis(KBMButtons))
			ret = false;
		prevInputMethod = ret;
		return ret;
	}

	/// <summary>
	/// Simplified method for checking multiple inputs against "0"
	/// </summary>
	/// <param name="input">list of axes to check against 0</param>
	/// <returns>true if any of them are true, false if none of them are true</returns>
	private static bool GetAxis(string[] input)
    {
		bool ret = false;
		for (int i = 0; i < input.Length; i++)
			ret |= Mathf.Abs(Input.GetAxisRaw(input[i])) > MonoBehaviourSingletonPersistent<Constants>.Instance.controlSwapThreashold;
		return ret;
    }

	/// <summary>
	/// used to fix the jump cancel by disabling the pin force when shooting
	/// </summary>
	/// <returns>true if gun has been fired</returns>
	public bool CheckForShoot()
	{
		return isGunRising;
	}

	public void UpdateGrounded()
	{
		List<ContactPoint2D> contacts = new List<ContactPoint2D>();
		penguinRb2d.GetContacts(contacts);
		contacts = contacts.FindAll(
			contact => (contact.collider.gameObject.CompareTag("Platform"))); // Find all contacts with ground objects
		contacts = contacts.FindAll(
			contact => Vector2.Dot(contact.normal.normalized, Vector2.up) >
		               MonoBehaviourSingletonPersistent<Constants>.Instance.standingLimit); // Find all contacts with
		                                                                                    // acceptable normals
		playerStateData.isGrounded = contacts.Count > 0; // If there are any, we are Grounded
		// Catches the first instance of hitting the ground for invuln timer
		if (playerStateData.isGrounded)
		{
			playerStateData.isStunnedFromKnockback = false;
			if (playerStateData.isStunnedFromKnockback == false && playerStateData.hitBouncePad == true)
			{
				// if both particles are not playing and you hit a bounce pad and the ground still play the particles
				if (penguinParticleSystem[0].isPlaying == false)
				{
                    penguinParticleSystem[0].Play();
                }
			}
   //         if (penguinParticleSystem[0].isPlaying == true)
			//{
   //             penguinParticleSystem[0].Stop(); // stops playing the particles when you touch the ground
				
			//}
			//if (penguinParticleSystem[5].isPlaying == true)
			//{
			//	penguinParticleSystem[5].Stop(); // stops playing the particles when you touch the ground

			//}
			//if (penguinParticleSystem[6].isPlaying == true && playerStateData.inWater != true)
			//{
			//	penguinParticleSystem[6].Stop();
			//}
		}

		// Debug.Log(playerStateData.isGrounded);
	}

	private void UpdateGroundAngle()
	{
		// Find all contacts with ground
		List<ContactPoint2D> contacts = new List<ContactPoint2D>();
		penguinRb2d.GetContacts(contacts);
		contacts = contacts.FindAll(
			contact => contact.collider.gameObject.CompareTag("Platform")); // Find all contacts with ground objects

		// Loop through ground contacts, find steepest contact angle
		float maxAngle = 0;
		foreach (ContactPoint2D contact in contacts)
		{
			maxAngle = Mathf.Max(maxAngle, Vector2.Angle(contact.normal, Vector2.up));
		}

		// save steepest contact angle
		currentWalkAngle = maxAngle;
	}
    /// <summary>
    /// When the penguin touches "Water", it changes him to the swimming state
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter2D(Collider2D other)
    {
		//Debug.LogError("OnTriggerEnter2D: Penguin in water");
		//Checks if the penguin entered the water object
		if (other.gameObject.CompareTag("Water") && playerStateData.inWater == false)
        {
            //Debug.LogError("Penguin in water");
            // Get a vector from penguin center to the water tube center
            tubeCenter = other.transform.position;
			//Acquire tube script
            SwimmingTube tubeScript = other.gameObject.GetComponent<SwimmingTube>();
			//Uses distance formula to figure out what side penguin is entering from
			//Then centers the penguin and sets the direction in which it's supposed to go
			if (Mathf.Sqrt((Mathf.Pow(transform.position.x - tubeScript.tubeSideA.x, 2) + (Mathf.Pow(transform.position.y - tubeScript.tubeSideA.y, 2)))) < Mathf.Sqrt((Mathf.Pow(transform.position.x - tubeScript.tubeSideB.x, 2) + (Mathf.Pow(transform.position.y - tubeScript.tubeSideB.y, 2)))))
			{
				transform.position = tubeScript.tubeSideA;
				swimDirection = tubeScript.tubeSideB - tubeScript.tubeSideA;
				tubeCenter = tubeScript.tubeSideA;
			}
			else
			{
				transform.position = tubeScript.tubeSideB;
				swimDirection = tubeScript.tubeSideA - tubeScript.tubeSideB;
				tubeCenter = tubeScript.tubeSideB;
			}
			//Sets the angle in which the penguin should face
            swimAngle = Mathf.Atan2(swimDirection.y, swimDirection.x) * Mathf.Rad2Deg;
			playerStateData.inWater = true;
            //Makes penguin face in correct angle
            swimAngle -= 90;
            penguinRb2d.MoveRotation(swimAngle);
			//Plays swimming animation
            penguinAnimator.SetBool("isSwimming", true);
			StartCoroutine(SpawnTrail(6));

			// update data
			DataTracker.Instance.numberOfTubesUsed++;
		}
	}

	/// <summary>
	/// Done when the rpenguin leaves the water
	/// </summary>
	/// <param name="other"></paramdra
	private void OnTriggerExit2D(Collider2D other)
	{
		// Checks if the penguin left the Water object
		if (other.gameObject.CompareTag("Water"))
		{
			playerStateData.inWater = false;
            penguinAnimator.SetBool("isSwimming", false);
            MonoBehaviourSingletonPersistent<ScoreManager>.Instance.IncreaseScore(MonoBehaviourSingletonPersistent<Constants>.Instance.waterTubePoints, TrickNames.WaterTube);
            MonoBehaviourSingletonPersistent<ScoreManager>.Instance.SpawnFloatingPoints(MonoBehaviourSingletonPersistent<Constants>.Instance.waterTubePoints);
            penguinRb2d.angularVelocity = 0;
		}
	}

	/// <summary>
	/// Movement code for when the penguin is in the "swimming" state
	/// </summary>
	public void Swim()
	{
		//Sets the penguin's velocity in direction of other side of tube.
        penguinRb2d.velocity = swimDirection * MonoBehaviourSingletonPersistent<Constants>.Instance.maxSwimSpeed;
	}

	/// <summary>
	/// Applies a force to the penguin while falling to allow for precise movements
	/// </summary>
	public void CheckForFallMove()
	{
		// Calculate fall redirect impulse
		float fallImpulse = playerStateData.horizInput *
		                    MonoBehaviourSingletonPersistent<Constants>.Instance.fallRedirect *
		                    penguinRb2d.mass; // Using impulse, don't need to normalize by time
		// Diminish force applied as penguin accelerates (inverse square)
		penguinRb2d.AddForce(new Vector2(fallImpulse / (Mathf.Pow(penguinRb2d.velocity.x, 2) + 1f), 0f),
		                     ForceMode2D.Impulse);
		// Force the penguin into the ground if the player presses the down key
		Vector2 downImpulse = playerStateData.vertInput < 0 ? Vector2.down : Vector2.zero;
		penguinRb2d.AddForce(downImpulse * MonoBehaviourSingletonPersistent<Constants>.Instance.downImpulse * penguinRb2d.mass, ForceMode2D.Impulse);
		isStomping = downImpulse == Vector2.zero ? false : true;
		//Debug.Log(isStomping);
	}
	/// <summary>
	/// Sets the penguin's direction to face towards the way he's falling
	/// </summary>
	public void SetFallDirection()
	{
		if (playerStateData.horizVel != 0 && !playerStateData.hitBouncePad)
		{
			float angle =
				Mathf.Atan2(playerStateData.playerYVel, Mathf.Abs(playerStateData.horizVel)) * Mathf.Rad2Deg;
            // Debug.Log(angle);
            angle -= 90;

            if (PlayerStateData.facingRight)
            {
                RotatePlayer(-angle);
            }
            else
            {
                RotatePlayer(angle);
            }      
        }
	}

	public void RotatePlayer(float desiredAngle)
	{
		penguinRb2d.angularVelocity = 0;

		Vector3 currentAngle = penguinTransform.rotation.eulerAngles;
		Quaternion rotation = Quaternion.Euler(0f, 0f, desiredAngle);
		penguinTransform.rotation =
			Quaternion.Lerp(penguinTransform.rotation, rotation,
		                    MonoBehaviourSingletonPersistent<Constants>.Instance.lerpSmoothing * Time.deltaTime);
		// Debug.Log(currentAngle);
	}

	/// <summary>
	/// Occurs when two colliders interact
	/// </summary>
	/// <param name="collision"></param>
	void OnCollisionEnter2D(Collision2D collision)
	{
		// Debug.Log("Player collided with " + collision.collider);

		if (collision.gameObject.CompareTag("BouncePad"))
		{
			// Debug.LogWarning("Bouncepad collision: " + Time.time);
			// Debug.Log("Relative velocity: " + collision.relativeVelocity);
			// Debug.Log("Countact count: " + collision.contactCount);
			// Debug.Log("1st contact normal impulse: " + collision.contacts[0].normalImpulse);

			// check if we are colliding fast enough to bounce
			if (Mathf.Abs(collision.GetContact(0).normalImpulse) >
			    MonoBehaviourSingletonPersistent<Constants>.Instance.wallBounceSpeedThreshold)
			{
				// Debug.Log(collision.relativeVelocity);
				playerStateData.hitBouncePad = true;
				UpdateBounceDir(collision.GetContact(0).normal);
			}
        }
		else if (collision.gameObject.CompareTag("Hazard"))
		{
			playerStateData.hitHazard = true;
		}
	}

	/// <summary>
	/// Occurs while two colliders continue interracting
	/// </summary>
	/// <param name="collision"></param>
	void OnCollisionStay2D(Collision2D collision)
	{
		// add force to the penguin rigidbody from the collision's normal force
		if (State == PlayerStateType.Moving && Grounded)
		{
			// Find all contacts with ground
			List<ContactPoint2D> contacts = new List<ContactPoint2D>();
			penguinRb2d.GetContacts(contacts);
			contacts = contacts.FindAll(
				contact => contact.collider.gameObject.CompareTag("Platform")); // Find all contacts with ground objects
			if (contacts.Count > 0)
			{
				playerStateData.lastGroundNormal = contacts[0].normal;
			}
		}
	}

	/// <summary>
	/// Bounces the penguin off of a bouncepad
	/// </summary>
	public void DoBounce()
	{
		playerStateData.hitBouncePad = false;
        // initailize variables
        Constants constants = MonoBehaviourSingletonPersistent<Constants>.Instance;
		float speed = constants.bounceSpeed;
		float x = Mathf.Cos(constants.wallBounceAngle * Mathf.Deg2Rad);
		float y = Mathf.Sin(constants.wallBounceAngle * Mathf.Deg2Rad);

		// Bounce player
		Vector2 vel = penguinRb2d.velocity;
		if (vel.x < 0 || (vel.x == 0 && lastBounceNorm.x < 0))
		{
			x *= -1;
		}
		y *= vel.y < 0 ? -1f : 1f;                        // flip y if moving down
		penguinRb2d.velocity = speed * new Vector2(x, y); // apply new velocity
		StartCoroutine(SpawnTrail(0));

		//Give bouncepad points
		MonoBehaviourSingletonPersistent<ScoreManager>.Instance.IncreaseScore(MonoBehaviourSingletonPersistent<Constants>.Instance.bouncePadPoints, TrickNames.BouncePad);
        MonoBehaviourSingletonPersistent<ScoreManager>.Instance.SpawnFloatingPoints(MonoBehaviourSingletonPersistent<Constants>.Instance.bouncePadPoints);

		// update data
		DataTracker.Instance.numberOfBouncepadsUsed++;

    }

	/// <summary>
	/// Pushes the penguin away and checks for invincibility, isRight true means left
	/// </summary>
	/// <param name="isRight"></param>
	public void OnKnockback(bool isRight)
	{
		// Only push the penguin if not being hit
		if (!playerStateData.hitHazard && invulnerableTimer <= 0)
		{
            playerStateData.hitHazard = true;

            damagedEffect = Instantiate(DamageParticle,transform.position, Quaternion.identity);
			damagedEffect.GetComponent<ParticleSystem>().Play();
			Destroy(damagedEffect,1f);

            // Zero out the velocities so that it moves predictably
            penguinRb2d.velocity = Vector3.zero;
			invulnerableTimer = 1f;
			penguinRb2d.angularVelocity = 0f;

			float force = playerStateData.facingRight ? MonoBehaviourSingletonPersistent<Constants>.Instance.knockbackForce
								  : -(MonoBehaviourSingletonPersistent<Constants>.Instance.knockbackForce);

			// Apply a force based on where the seal was touched
			penguinRb2d.AddForce(
				new Vector2(force, MonoBehaviourSingletonPersistent<Constants>.Instance.knockbackForce),
				ForceMode2D.Impulse);

            // Sets the animator's trigger off in order to play the animator
            
            penguinAnimator.SetTrigger("isDamaged");
			// Debug.LogError("Penguin::OnKnockback - pc == " + pc);
			pc.RemovePearls();
			pearlTimeCurr = pearlTimeMax;
			isUnableToCollectPearls = true;
		}
	}

	/// <summary>
	/// Ticks player's invulnerability timer to ensure the player cannot be hit back-to-back by hazards
	/// </summary>
	private void InvulnTimer()
	{
		if (invulnerableTimer > 0)
		{
			invulnerableTimer -= Time.deltaTime;
		}
		else
		{
			invulnerableTimer = 0f;
			playerStateData.hitHazard = false;
		}
	}

	private void PearlTimer()
	{
		if(pearlTimeCurr > 0)
		{
			pearlTimeCurr -= Time.deltaTime;
		}
		else
		{
			isUnableToCollectPearls = false;
		}
	}

	/// <summary>
	/// </summary>
	/// Resets InvulnTimer so that penguin can break icicles again after shooting or hitting a bouncepad/flipper
	public void TurnOffHitHazard()
	{
        playerStateData.hitHazard = false;
    }

	public void DisableSliding()
	{
		playerStateData.isSliding = false;
		penguinParticleSystem[1].Stop();
        penguinParticleSystem[2].Stop();
		penguinParticleSystem[4].Stop();

        // Stop sliding sound
        AudioManager.Instance.Pause(Sounds.PlayerSlide);
	}

	public void LevelCompleted()
	{
		hasFinishedLevel = true;
    }

	public override void NotifyLevelStart()
    {
		levelStartCooldown = false;
    }

    /// <summary>
    /// Updates penguin's particles if they should be playing
    /// </summary>
    void UpdateSlideParticles()
    {
        //components for particle system
        var main = penguinParticleSystem[1].main;
        var emission = penguinParticleSystem[1].emission;
		var angle = penguinParticleSystem[1].transform;
		ParticleSystem.ShapeModule shape = penguinParticleSystem[1].shape;
		float currSpeed = Mathf.Abs(penguinRb2d.velocity.magnitude);

        //plays particles when at kill speed
        if (currSpeed >= MonoBehaviourSingletonPersistent<Constants>.Instance.maxWalkSpeed && playerStateData.isSliding)
		{
            if (currSpeed >= MonoBehaviourSingletonPersistent<Constants>.Instance.minCollisionEnemyDeathSpeed)
			{
                penguinParticleSystem[1].Play();
                penguinParticleSystem[2].Play();
				penguinParticleSystem[4].Play();
			}
			else if (currSpeed <= MonoBehaviourSingletonPersistent<Constants>.Instance.minCollisionEnemyDeathSpeed)
            {
				penguinParticleSystem[1].Stop();
				penguinParticleSystem[2].Stop();
				penguinParticleSystem[4].Stop();
			}

			//if (!speedSoundPlayed)
			//{
			//	speedSoundPlayed = true;
			//	AudioManager.Instance.Play(Sounds.KillingSpeed);
			//}
		}
        //stops particles when not at kill speed
        else if ((currSpeed < MonoBehaviourSingletonPersistent<Constants>.Instance.maxWalkSpeed || !playerStateData.isSliding))
        {
            penguinParticleSystem[1].Stop();
			penguinParticleSystem[2].Stop(); 
			penguinParticleSystem[3].Stop();
			penguinParticleSystem[4].Stop();
            emission.rateOverDistance = 75;
			shape.angle = 5;
			speedSoundPlayed = false;
        }

        if (penguinParticleSystem[1].isEmitting)
		{
            emission.rateOverDistanceMultiplier += 2;
			shape.angle += .5f;
			if(shape.angle >= 25)
			{
				shape.angle = 25;
			}
            
        }
        
    }

	/// <summary>
	/// Makes it so that the flipper can play the penguin's stream particle
	/// </summary>
	public void PlayFlipperStream()
	{
		//Debug.LogError("PlayFlipperStream");
		playerStateData.isSliding = false;
		playerStateData.isGrounded = false;
		StartCoroutine(SpawnTrail(5));
    }

	private IEnumerator SpawnTrail(int effectNbr)
    {
		penguinParticleSystem[0].Stop(); // stops playing the particles for bouncepad
		penguinParticleSystem[5].Stop(); // stops playing the particles for Flipper
		penguinParticleSystem[6].Stop(); // stops playing the particles for water tube

		penguinParticleSystem[effectNbr].Play();  // plays the white trail

		yield return new WaitForSeconds(1.0f);

		penguinParticleSystem[effectNbr].Stop();  // plays the white trail
	}


}
