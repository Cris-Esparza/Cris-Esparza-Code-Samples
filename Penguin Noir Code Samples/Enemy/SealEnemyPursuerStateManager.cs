using UnityEngine;

public class SealEnemyPursuerStateManager : SealEnemyStateManager
{
    
    private EnemyLookLeftState lookLeft;    // strategy for look left state
    private EnemyLookRightState lookRight;  // strategy for look right state
    private EnemyAlertState alert;          // strategy for alert / pause state state
    private EnemyPursueState pursue;        // strategy for pursue state
    private EnemyConfuseState confuse;      // strategy for confuse state
    private EnemyResetState reset;          // strategy for look right state
    private EnemyDyingState dying;

    private int lookDirCounter = 0;         // frame count enemy has been looking in current direction
    private int alertCounter = 0;           // frame count enemy has been waiting to start pursuing
    private int confuseCounter = 0;
    private int pursuitCounter = 0;         // frame count enemy has been pursuing withough seeing player
    private int dyingCounter = 0;

    /// <summary>
    /// Creates a new state manager of type "pursuer" for the given enemy
    /// </summary>
    /// <param name="enemy">The enemies state to manage</param>
    public SealEnemyPursuerStateManager(Enemy enemy)
    {
        this.enemy = enemy;

        // Initialize all flyweight objects
        lookLeft = new EnemyLookLeftState(enemy);
        lookRight = new EnemyLookRightState(enemy);
        alert = new EnemyAlertState(enemy);
        pursue = new EnemyPursueState(enemy);
        confuse = new EnemyConfuseState(enemy);
        reset = new EnemyResetState(enemy);
        dying = new EnemyDyingState(enemy);
    }

    public override EnemyState GetNextState(Enemy enemy)
    {
        EnemyState nextState = null;

        switch (enemy.State.GetStateType())
        {
            case EnemyStateType.Init:
                nextState = lookLeft;
                break;
            case EnemyStateType.LookLeft:
                if (enemy.IsDying) // killed by player -> dying
                {
                    nextState = dying;
                }
                else if (enemy.CanSeePlayer) // player found -> alert
                {
                    nextState = alert;
                    lookDirCounter = 0;
                }
                else if (lookDirCounter < MonoBehaviourSingletonPersistent<Constants>.Instance.enemyLookDuration) // can't see player -> wait to change sides
                {
                    lookDirCounter++;
                    nextState = lookLeft;
                }
                else // can't see player + wait over -> change sides
                {
                    lookDirCounter = 0;
                    nextState = lookRight;
                }
                break;
            case EnemyStateType.LookRight: // player found -> alert
                if (enemy.IsDying) // killed by player -> dying
                {
                    nextState = dying;
                }
                else if (enemy.CanSeePlayer)
                {
                    nextState = alert;
                    lookDirCounter = 0;
                }
                else if (lookDirCounter < MonoBehaviourSingletonPersistent<Constants>.Instance.enemyLookDuration) // can't see player -> wait to change sides
                {
                    lookDirCounter++;
                    nextState = lookRight;
                }
                else // can't see player + wait over -> change sides
                {
                    lookDirCounter = 0;
                    nextState = lookLeft;
                }
                break;
            case EnemyStateType.Alert:
                if (enemy.IsDying) // killed by player -> dying
                {
                    nextState = dying;
                }
                else if (alertCounter < MonoBehaviourSingletonPersistent<Constants>.Instance.enemyAlertDuration) // wait before pursuit -> alert
                {
                    alertCounter++;
                    nextState = alert;
                }
                else // wait finished -> pursue
                {
                    alertCounter = 0;
                    nextState = pursue;
                }
                break;
            case EnemyStateType.Pursue:
                if (enemy.IsDying) // killed by player -> dying
                {
                    nextState = dying;
                }
                else if (enemy.HittingPlayer()) // hit player -> return to station
                {
                    pursuitCounter = 0;
                    nextState = reset;
                }
                else if (enemy.CanSeePlayer) // can see player -> pursue
                {
                    
                    if (!enemy.lookAhead() || enemy.hitWall() || enemy.hitEnemy()) //Checks if enemy will hit a wall or fall off
                    {
                        nextState = confuse;
                    }
                    else
                    {
                        pursuitCounter = 0;
                        nextState = pursue;
                    }
                }
                else if (pursuitCounter < MonoBehaviourSingletonPersistent<Constants>.Instance.enemyPursuitDuration) // can't see player, but did see recently -> pursue last known location
                {
                    if (!enemy.lookAhead() || enemy.hitWall() || enemy.hitEnemy())//Checks if enemy will hit a wall or fall off
                    {
                        nextState = confuse;
                        
                    }
                    else
                    {
                        pursuitCounter++;
                        nextState = pursue;
                    }
                    
                }
                else // lost player completely -> confused
                {
                    nextState = confuse;
                }
                break;
            case EnemyStateType.Confuse:
                if (enemy.IsDying) // killed by player -> dying
                {
                    nextState = dying;
                }
                else if (enemy.CanSeePlayer) // found player again -> pursue
                {
                    confuseCounter = 0;
                    nextState = pursue;
                }
                else if (confuseCounter < MonoBehaviourSingletonPersistent<Constants>.Instance.enemyConfuseDuration) // still searching for player -> confuse
                {
                    
                    
                        confuseCounter++;
                        nextState = confuse;
                    
                }
                else // forget about player -> reset
                {
                    confuseCounter = 0;
                    nextState = reset;
                }
                break;
            case EnemyStateType.Reset:
                if (enemy.IsDying) // killed by player -> dying
                {
                    nextState = dying;
                }
                else if (enemy.LookForHome()) // close to start point -> reset to looking
                {
                    nextState = lookLeft;
                }
                else // not at start point -> keep resetting
                {
                    nextState = reset;
                }
                break;
            case EnemyStateType.Dying:
                if (dyingCounter < MonoBehaviourSingletonPersistent<Constants>.Instance.enemyDeathDuration)
                {
                    dyingCounter++;
                    nextState = dying;
                }
                else // game object destroyed on transition from dying, next state irrelevant 
                {
                    nextState = lookLeft;
                }
                break;
            default:
                //Debug.LogError("Unrecognized enemy state!");
                break;
        }

         //Debug.Log(nextState);

        return nextState;
    }
}
