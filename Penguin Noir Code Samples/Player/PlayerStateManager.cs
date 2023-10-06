using Unity.VisualScripting;
using UnityEngine;

/***
 *  Brendan Gould
 *  18 Sept 2022
 *  bgould2@uccs.edu
 *
 *  State manager for the player. Determines which state player should enter
 ***/

/// <summary>
/// Implements logic to decide when to transition from one state to another
/// </summary>
public class PlayerStateManager
{
    // Constants
    [SerializeField]
    private const float eps = .8f; // small number used for approximate equality checking
    [SerializeField]
    private readonly float slowSpeed; // speed below which player is moving "slowly"
    [SerializeField]
    private int rotationFallingCounter = 0; // current number of frames without grounded while sliding
    private int bouncingCounter = 0; // current number of frames in bouncing state after leaving wall

    // flyweights
    private Penguin player; // the player to manage state for
    private PlayerStandingState standingState; // strategy for standing state
    private PlayerMovingState movingState;
    private PlayerBouncingState bouncingState;
    private PlayerSwimmingState swimmingState; // strategy for swimming state
    private PlayerRisingState risingState; // strategy for rising state
    private PlayerFallingState fallingState; // strategy for falling state
    private PlayerKnockbackState knockbackState; // strategy for invulnerable state
    private PlayerCutsceneState cutsceneState; //strategy for death state
    private PlayerLevelEndState endLevelState; //stategy for ending level state
    private PlayerDialogState dialogState;

    // for animator
    Animator penguinAnimator;

    public PlayerStateManager(Penguin player)
    {
        this.player = player;
        slowSpeed = MonoBehaviourSingletonPersistent<Constants>.Instance.maxWalkSpeed;

        // Initialize all flyweight objects
        standingState = new PlayerStandingState(player);
        movingState = new PlayerMovingState(player);
        bouncingState = new PlayerBouncingState(player);
        swimmingState = new PlayerSwimmingState(player);
        risingState = new PlayerRisingState(player);
        fallingState = new PlayerFallingState(player);
        knockbackState = new PlayerKnockbackState(player);
        endLevelState = new PlayerLevelEndState(player);
        cutsceneState = new PlayerCutsceneState(player);
        dialogState = new PlayerDialogState(player);
    }

    /// <summary>
    /// Handles animator transitions between states
    /// </summary>
    /// <param name="nextState"></param>
    void SetAnimatorParams(PlayerStateType oldState, PlayerState newState)
    {
        // penguin textured object is a child of player object
        penguinAnimator = player.GetComponentInChildren<Animator>();

        penguinAnimator.SetBool("isIdle", newState == standingState || newState == dialogState);
        penguinAnimator.SetBool("isWalking", newState == movingState && !player.PlayerStateData.isSliding);
        penguinAnimator.SetBool("isSliding", newState == movingState && player.PlayerStateData.isSliding);
        penguinAnimator.SetBool("isRising", newState == risingState);
        penguinAnimator.SetBool("isFalling", newState == fallingState);
        penguinAnimator.SetBool("isDamaged", newState == knockbackState);
        penguinAnimator.SetBool("isSwimming", newState == swimmingState);
        penguinAnimator.SetFloat("isFastEnough", Mathf.Abs(player.PenguinRb2d.velocity.x));
        penguinAnimator.SetFloat("yVelocity", Mathf.Abs(player.PenguinRb2d.velocity.y));
        penguinAnimator.SetFloat("bounceCounter", bouncingCounter);

        if (oldState != PlayerStateType.Bouncing &&
            newState == bouncingState)
        {
            penguinAnimator.ResetTrigger("isDeflating");
            penguinAnimator.SetTrigger("isInflating");
        }

        if (oldState == PlayerStateType.Bouncing &&
            newState != bouncingState)
        {
            penguinAnimator.SetTrigger("isDeflating");
            bouncingCounter = 0;
        }
    }

    /// <summary>
    /// Calculates next state player should enter, given current data about him
    /// </summary>
    public PlayerState GetNextState(Penguin player, PlayerStateData data)
    {

        data.horizVel = player.GetComponent<Rigidbody2D>().velocity.x;

        PlayerState nextState = null;

        if (data.hitHazard) // Forces invuln state after player is hit
        {
            data.stateType = PlayerStateType.Knockback;
        }
        else if(DialogManager.isActive)
        {
            data.stateType = PlayerStateType.Dialog;
        }

        switch (data.stateType)
        {
            case PlayerStateType.EndLevel:
                nextState = endLevelState;
                break;
            case PlayerStateType.Standing:
                if (player.hasFinishedLevel == true)
                {
                    nextState = endLevelState;
                }
                else if (data.hitHazard)
                {
                    nextState = knockbackState;
                }
                else if (data.gunRisingInput || (Mathf.Abs(data.playerYVel) > Mathf.Abs(data.horizVel) + 5f && !data.isGrounded))
                {
                    nextState = risingState;
                }
                else if (data.isGrounded)
                {
                    if (data.horizInput != 0 || Mathf.Abs(data.horizVel) > eps) // want to move horizontally -> walking
                    {
                        nextState = movingState;
                    }
                    else // doesn't want to slide or move -> standing
                    {
                        nextState = standingState;
                    }
                }
                else if (data.hitBouncePad)
                {
                    nextState = bouncingState;
                    bouncingCounter = 0;
                }
                else // not on ground -> if y is positive, rising, else, falling
                {
                    if (data.playerYVel > 0 && !data.isGrounded)
                    {
                        nextState = risingState;
                    }
                    else if (!data.isGrounded)
                    {
                        nextState = fallingState;
                    }
                    else
                    {
                        nextState = movingState;
                    }
                }
                break;
            case PlayerStateType.Moving:
                if (player.hasFinishedLevel == true)
                {
                    nextState = endLevelState;
                }
                else if (data.inWater)
                {
                    nextState = swimmingState;
                    rotationFallingCounter = 0; // reset counter
                }
                else if (data.hitHazard)
                {
                    nextState = knockbackState;
                }
                else if (data.hitBouncePad)
                {
                    nextState = bouncingState;
                    bouncingCounter = 0;
                }
                else if (data.isGrounded)
                {
                    if (data.horizInput == 0 && Mathf.Abs(data.horizVel) < eps)
                    {
                        nextState = standingState;
                    }
                    else
                    {
                        nextState = movingState;
                    }
                    rotationFallingCounter = 0; // reset counter
                }
                else
                {
                    if (rotationFallingCounter < MonoBehaviourSingletonPersistent<Constants>.Instance.maxRotationFallingFrames)
                    {
                        // Wait + increment counter
                        nextState = movingState;
                        rotationFallingCounter++;
                    }
                    else
                    {
                        if (data.playerYVel > 0)
                        {
                            nextState = risingState;
                        }
                        else
                        {
                            nextState = fallingState;
                        }
                    }
                }
                break;
            case PlayerStateType.Swimming:
                if (player.hasFinishedLevel == true)
                {
                    nextState = endLevelState;
                }
                else if (data.inWater) // in water -> swim
                {
                    nextState = swimmingState;
                }
                else // no longer in water -> rising or falling based on y vel
                {
                    if (data.playerYVel > 0)
                    {
                        nextState = risingState;
                    }
                    else if (data.playerYVel < 0)
                    {
                        nextState = fallingState;
                    }
                    else
                    {
                        nextState = movingState;
                    }
                }
                break;
            case PlayerStateType.Rising:
                if (player.hasFinishedLevel == true)
                {
                    nextState = endLevelState;
                }
                else if (data.inWater) // in water -> swim
                {
                    nextState = swimmingState;
                }
                else if (data.hitHazard)
                {
                    nextState = knockbackState;
                }
                else if (data.hitBouncePad)
                {
                    nextState = bouncingState;
                    bouncingCounter = 0;
                }
                else if (data.isGrounded)
                {
                    nextState = movingState;
                }
                else // not in water or on ground -> falling
                {
                    if (data.playerYVel > 0)
                    {
                        nextState = risingState;
                    }
                    else
                    {
                        nextState = fallingState;
                    }
                }
                break;
            case PlayerStateType.Falling:
                if (player.hasFinishedLevel == true)
                {
                    nextState = endLevelState;
                }
                else if (data.inWater) // in water -> swim
                {
                    nextState = swimmingState;
                }
                else if (data.hitHazard)
                {
                    nextState = knockbackState;
                }
                else if (data.hitBouncePad)
                {
                    nextState = bouncingState;
                    bouncingCounter = 0;
                }
                else if (data.isGrounded)
                {
                    if (data.horizInput == 0 && Mathf.Abs(data.horizVel) < eps) // not moving horizontally or sliding -> standing
                    {
                        nextState = standingState;
                    }
                    else
                    {
                        nextState = movingState;
                    }
                }
                else // not in water or on ground -> falling
                {
                    if (data.playerYVel > 0 || data.gunRisingInput)
                    {
                        nextState = risingState;
                    }
                    else
                    {
                        nextState = fallingState;
                    }
                }
                break;
            case PlayerStateType.Knockback:
                if (player.hasFinishedLevel == true)
                {
                    nextState = endLevelState;
                }
                else if (data.inWater) // in water -> swim
                {
                    nextState = swimmingState;
                }
                else if (data.hitBouncePad)
                {
                    nextState = bouncingState;
                    bouncingCounter = 0;
                }
                else if (data.hitHazard)
                {
                    nextState = knockbackState;
                }
                else if (data.isGrounded)
                {
                    if (data.horizInput == 0 && Mathf.Abs(data.horizVel) < eps) // not moving horizontally or sliding -> standing
                    {
                        nextState = standingState;
                    }
                    else
                    {
                        nextState = movingState;
                    }
                }
                else // not in water or on ground -> falling
                {
                    if (data.playerYVel > 0)
                    {
                        nextState = risingState;
                    }
                    else if (data.playerYVel < 0)
                    {
                        nextState = fallingState;
                    }
                    else
                    {
                        nextState = knockbackState;
                    }
                }
                break;
            case PlayerStateType.Bouncing:
                if (player.hasFinishedLevel == true)
                {
                    nextState = endLevelState;
                }
                else if (data.inWater) // in water -> swim
                {
                    nextState = swimmingState;
                }
                else if (data.hitHazard)
                {
                    nextState = knockbackState;
                }
                else if (data.hitBouncePad)
                {
                    nextState = bouncingState;
                    bouncingCounter = 0;
                }
                else if (bouncingCounter < MonoBehaviourSingletonPersistent<Constants>.Instance.maxBouncingFrames)
                { // wait + increment counter
                    nextState = bouncingState;
                    bouncingCounter++;
                }
                else
                {
                    if (data.horizInput == 0 && Mathf.Abs(data.horizVel) < eps)
                    {
                        nextState = standingState;
                    }
                    else if (data.isGrounded || data.isSliding)
                    {
                        nextState = movingState;
                    }
                    else
                    {
                        if (data.playerYVel > 0)
                        {
                            nextState = risingState;
                        }
                        else
                        {
                            nextState = fallingState;
                        }
                    }
                }
                break;
            case PlayerStateType.Cutscene:
                nextState = cutsceneState;
                break;
            case PlayerStateType.Dialog:
                if(!DialogManager.isActive)
                {
                    nextState = standingState;
                }
                else
                {
                    nextState = dialogState;
                }
                break;
            default:
                Debug.LogError("Unrecognized player state!");
                break;
        }

        SetAnimatorParams(data.stateType, nextState);
        // Debug.Log(nextState);

        return nextState;
    }
}
