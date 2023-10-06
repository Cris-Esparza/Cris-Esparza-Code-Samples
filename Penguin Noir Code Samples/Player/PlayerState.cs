using UnityEngine;

/***
 *  Brendan Gould
 *  18 Sept 2022
 *  bgould2@uccs.edu
 *
 *  Nathan Peckham
 *  9/29/22
 *  npeckham@uccs.edu
 *  
 *  Base class for player states
 *  
 *  added norm gravity scale -N
 ***/

/// <summary>
/// Vars to store data about current player state
/// </summary>
public enum PlayerStateType
{
    Standing,
    Moving,
    Swimming,
    Rising,
    Falling,
    Knockback,
    Bouncing,
    Death,
    Dialog,
    Cutscene,
    EndLevel
}

/// <summary>
/// Implements logic to decide when to transition from one state to another
/// </summary>
public abstract class PlayerState : StateMachineState
{
    protected Penguin player;
    protected Rigidbody2D penguinRb2d;

    /// <summary>
    /// Set reference to player for later use
    /// </summary>
    protected PlayerState(Penguin player)
    {
        this.player = player;
        penguinRb2d = player.GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Gets the state type enum associated with this state
    /// </summary>
    public abstract PlayerStateType GetStateType();
}
