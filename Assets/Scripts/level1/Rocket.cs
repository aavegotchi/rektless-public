using enemies;
using UnityEngine;

namespace level2
{
    using RocketType = BaseEnemy<RocketConfig, RocketComponents>;

    public class Rocket : RocketType, IStompable
    {
        protected override void InitializeStateMachine()
        {
            var idleState = new CommonIdleState<RocketType>();
            var patrolState = new CommonPatrolState<RocketType>();
            var attackState = new CommonAttackState<RocketType>();
            var takeDamageState = new RocketTakeDamageState();
            var deathState = new CommonDeathState<RocketType>();

            stateMachine.AddState(idleState);
            stateMachine.AddState(patrolState);
            stateMachine.AddState(attackState);
            stateMachine.AddState(takeDamageState);
            stateMachine.AddState(deathState);

            stateMachine.AddTransition(idleState, patrolState, () => idleState.IdleTimeElapsed);
            stateMachine.AddTransition(patrolState, attackState, () => patrolState.PlayerNearby);
            stateMachine.AddTransition(attackState, () =>
            {
                if (attackState.AttackComplete)
                {
                    return config.ReturnToPatrolAfterAttack ? patrolState : idleState;
                }

                return null;
            });

            stateMachine.AddTransition(takeDamageState, () =>
            {
                if (takeDamageState.DamageAnimationComplete)
                {
                    return takeDamageState.GetStateToReturnTo();
                }

                return null;
            });

            State<RocketType>[] allStates = { idleState, patrolState, attackState };
            foreach (var state in allStates)
            {
                stateMachine.AddTransition(state, takeDamageState, () => isTakingDamage);
            }

            stateMachine.SetInitialState<CommonIdleState<RocketType>>();
        }

        public override BoxCollider2D GetAttackCollider() =>
            config.PlayerNearbyCollider; // Rocket doesn't have a separate attack collider

        public override void CompleteTakeDamage()
        {
            isTakingDamage = false;
            if (stateMachine.CurrentState is RocketTakeDamageState takeDamageState)
            {
                takeDamageState.OnTakeDamageComplete();
            }
        }

        public override void OnAttackTrigger()
        {
            components.AudioSource.PlayOneShot(config.AttackSound);
            Instantiate(config.BulletPrefab, config.BulletSpawnPoint.position, transform.rotation);
        }

        public override void OnAttackComplete()
        {
            if (stateMachine.CurrentState is CommonAttackState<RocketType> attackState)
            {
                attackState.OnAttackComplete();
            }
        }

        public override void OnPlayerAttack()
        {
            stateMachine.ChangeState<RocketTakeDamageState>();
        }

        public new void OnStomped() => OnPlayerAttack();
    }

    public class RocketTakeDamageState : State<RocketType>
    {
        public bool DamageAnimationComplete { get; private set; }
        private State<RocketType> stateToReturnTo;

        public override void Enter()
        {
            DamageAnimationComplete = false;
            owner.TakeDamage();
            if (owner.GetLives() <= 0 && stateMachine.HasState<CommonDeathState<RocketType>>())
            {
                stateMachine.ChangeState<CommonDeathState<RocketType>>();
                return;
            }

            stateToReturnTo = DetermineStateToReturnTo();
        }

        public override void Exit()
        {
            DamageAnimationComplete = false;
        }

        public void OnTakeDamageComplete()
        {
            DamageAnimationComplete = true;
        }

        private State<RocketType> DetermineStateToReturnTo()
        {
            var previousState = stateMachine.PreviousState;
            if (previousState is CommonAttackState<RocketType>)
            {
                owner.GetAnimator().SetTrigger("takedamage");
                return owner.Config.ReturnToPatrolAfterAttack
                    ? stateMachine.GetState<CommonPatrolState<RocketType>>()
                    : stateMachine.GetState<CommonIdleState<RocketType>>();
            }

            if (previousState is RocketTakeDamageState previousTakeDamage)
            {
                owner.GetAnimator().Play("takedamage", -1, 0);
                return previousTakeDamage.GetStateToReturnTo();
            }

            owner.GetAnimator().SetTrigger("takedamage");
            return previousState;
        }

        public State<RocketType> GetStateToReturnTo()
        {
            return stateToReturnTo;
        }
    }

    [System.Serializable]
    public class RocketConfig : EnemyConfig
    {
        public Transform BulletSpawnPoint;
        public GameObject BulletPrefab;
        public bool ReturnToPatrolAfterAttack = true;
    }

    [System.Serializable]
    public class RocketComponents : EnemyComponents
    {
    }
}