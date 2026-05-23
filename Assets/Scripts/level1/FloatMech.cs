using enemies;

namespace level2
{
    using Unity.VisualScripting.FullSerializer;
    using UnityEngine;
    using FloatMechType = BaseEnemy<FloatMechConfig, FloatMechComponents>;

    public class FloatMech : FloatMechType, IStompable
    {
        protected override void InitializeStateMachine()
        {
            var patrolState = new FlyingPatrolState<FloatMechType>();
            var attackState = new FlyingAttackState<FloatMechType>();
            var takeDamageState = new FlyingTakeDamageState<FloatMechType>();
            var deathState = new FlyingDeathState<FloatMechType>();

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

            stateMachine.SetInitialState<FlyingPatrolState<FloatMechType>>();
        }

        public override void OnPlayerAttack()
        {
            stateMachine.ChangeState<FlyingTakeDamageState<FloatMechType>>();
        }

        public override void CompleteTakeDamage()
        {
            isTakingDamage = false;
            if (stateMachine.CurrentState is FlyingTakeDamageState<FloatMechType> takeDamageState)
            {
                takeDamageState.OnTakeDamageComplete();
            }
        }

        public override void OnAttackComplete()
        {
            if (stateMachine.CurrentState is FlyingAttackState<FloatMechType> attackState)
            {
                attackState.OnAttackComplete();
            }
        }

        public override void Die() => stateMachine.ChangeState<FlyingDeathState<FloatMechType>>();
    }



    [System.Serializable]
    public class FloatMechConfig : EnemyConfig
    {
    }

    [System.Serializable]
    public class FloatMechComponents : EnemyComponents
    {
    }
}