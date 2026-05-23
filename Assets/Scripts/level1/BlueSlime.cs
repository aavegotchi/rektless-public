using enemies;

namespace level2
{
    using UnityEngine;
    using BlueSlimeType = BaseEnemy<BlueSlimeConfig, BlueSlimeComponents>;

    public class BlueSlime : BlueSlimeType, IStompable
    {
        protected override void InitializeStateMachine()
        {
            var patrolState = new CommonPatrolState<BlueSlimeType>();
            var attackState = new CommonAttackState<BlueSlimeType>();
            var takeDamageState = new CommonTakeDamageState<BlueSlimeType>();
            var deathState = new CommonDeathState<BlueSlimeType>();

            stateMachine.AddState(patrolState);
            stateMachine.AddState(attackState);
            stateMachine.AddState(takeDamageState);
            stateMachine.AddState(deathState);


            stateMachine.AddTransition(takeDamageState, () =>
            {
                if (takeDamageState.DamageAnimationComplete)
                {
                    return takeDamageState.GetPreviousState();
                }

                return null;
            });

            stateMachine.AddTransition(attackState, patrolState, () => attackState.AttackComplete);

            stateMachine.AddTransition(patrolState, takeDamageState, () => isTakingDamage);
            stateMachine.AddTransition(patrolState, attackState, () =>
            {
                Vector2 toPlayer = Player.Instance.transform.position - transform.position;
                return toPlayer.sqrMagnitude <= 15f
                && Mathf.Sign(toPlayer.x) == -Mathf.Sign(transform.right.x);
            });

            stateMachine.SetInitialState<CommonPatrolState<BlueSlimeType>>();
        }

        public void OnAttackAnimationEnd()
        {
            stateMachine.GetState<CommonAttackState<BlueSlimeType>>().EndState();
        }
    }

    [System.Serializable]
    public class BlueSlimeConfig : EnemyConfig
    {
    }

    [System.Serializable]
    public class BlueSlimeComponents : EnemyComponents
    {
    }
}