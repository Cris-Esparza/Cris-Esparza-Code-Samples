
using UnityEngine;

public class SealEnemySearchStateManager : SealEnemyStateManager
{
    private EnemySearchLeftState searchLeft;     // strategy for searching left state
    private EnemySearchRightState searchRight;  //strategy for searching right state
    private EnemyLookRightState lookRight;      // strategy for look right state
    private EnemyAlertState alert;          // strategy for alert / pause state state
    private EnemyPursueState pursue;        // strategy for pursue state
    private EnemyConfuseState confuse;      // strategy for confuse state
    private EnemyResetState reset;          // strategy for look right state
    private EnemyDyingState dying;

    private int alertCounter = 0;           // frame count enemy has been waiting to start pursuing
    private int confuseCounter = 0;
    private int pursuitCounter = 0;         // frame count enemy has been pursuing withough seeing player
    private int dyingCounter = 0;

    /// <summary>
    /// Creates a new state manager of type "searcher" for the given enemy
    /// </summary>
    /// <param name="enemy">The enemies state to manage</param>
    public SealEnemySearchStateManager(Enemy enemy)
    {
        this.enemy = enemy;

        // Initialize all flyweight objects
        lookRight = new EnemyLookRightState(enemy);
        searchLeft = new EnemySearchLeftState(enemy);
        searchRight = new EnemySearchRightState(enemy);
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
            case EnemyStateType.Init: //Inital enemy state change
                nextState = searchLeft;
                break;
            case EnemyStateType.SearchLeft:
                if (enemy.IsDying) // killed by player -> dying
                {
                    nextState = dying;
                }
                else if (enemy.CanSeePlayer) // player found -> alert
                {
                    nextState = alert;
                }
                else if (enemy.hitWall() || !enemy.lookAhead() || enemy.hitEnemy() ||
                         enemy.transform.position.x <= enemy.RandomSearchLeft.x) // if the enemy has not reached the end of its search
                {
                    nextState = searchRight;
                }
                else
                {
                    nextState = searchLeft;
                }
                break;
            case EnemyStateType.SearchRight: // player found -> alert
                if (enemy.IsDying) // killed by player -> dying
                {
                    nextState = dying;
                }
                else if (enemy.CanSeePlayer)
                {
                    nextState = alert;
                    
                }
                else if (enemy.hitWall() || !enemy.lookAhead() || enemy.hitEnemy() ||
                         enemy.transform.position.x >= enemy.RandomSearchRight.x) // can't see player -> wait to change sides
                {
                    nextState = searchLeft;
                }
                else
                {

                    nextState = searchRight;
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
                    pursuitCounter = 0;
                    nextState = pursue;
                }
                else if (pursuitCounter < MonoBehaviourSingletonPersistent<Constants>.Instance.enemyPursuitDuration || enemy.hitWall() || !enemy.lookAhead() || enemy.hitEnemy()) // can't see player, but did see recently -> pursue last known location
                {
                    pursuitCounter++;
                    nextState = pursue;
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
                else if (confuseCounter < MonoBehaviourSingletonPersistent<Constants>.Instance.enemyConfuseDuration || enemy.hitWall() || !enemy.lookAhead() || enemy.hitEnemy()) // still searching for player -> confuse
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
                    nextState = searchLeft;
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
                    nextState = searchLeft;
                }
                break;
            default:
                Debug.LogError("Unrecognized enemy state!");
                break;
        }

        Debug.Log(nextState);

        return nextState;
    }
}
