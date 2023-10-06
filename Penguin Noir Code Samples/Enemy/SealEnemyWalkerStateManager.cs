using System.Net.NetworkInformation;
using UnityEngine;

public class SealEnemyWalkerStateManager : SealEnemyStateManager
{

    private EnemyWalkLeftState walkLeft;
    private EnemyWalkRightState walkRight;
    private EnemyDyingState dying;

    private int dyingCounter = 0;

    /// <summary>
    /// Creates a new state manager of type "pursuer" for the given enemy
    /// </summary>
    /// <param name="enemy">The enemies state to manage</param>
    public SealEnemyWalkerStateManager(Enemy enemy)
    {
        this.enemy = enemy;

        // Initialize all flyweight objects
        walkLeft = new EnemyWalkLeftState(enemy);
        walkRight = new EnemyWalkRightState(enemy);
        dying = new EnemyDyingState(enemy);

    }

    public override EnemyState GetNextState(Enemy enemy)
    {
        EnemyState nextState = null;
        switch (enemy.State.GetStateType())
        {
            case EnemyStateType.Init:
                nextState = walkRight;
                break;
            case EnemyStateType.WalkLeft: // Walk to the left
                if (enemy.IsDying) // killed by player -> dying
                {
                    nextState = dying;
                }
                else if (!enemy.lookAhead() || enemy.hitWall() || enemy.hitEnemy())
                {
                    nextState = walkRight;
                }
                else
                {
                    nextState = walkLeft;
                }
                break;
            case EnemyStateType.WalkRight: // Walk to the right
                if (enemy.IsDying) // killed by player -> dying
                {
                    nextState = dying;
                }
                else if (!enemy.lookAhead() || enemy.hitWall() || enemy.hitEnemy())
                {
                    nextState = walkLeft;
                }
                else
                {
                    nextState = walkRight;
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
                    nextState = walkRight;
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
