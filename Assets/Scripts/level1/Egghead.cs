using enemies;
using UnityEngine;

namespace level2
{
    using EggheadType = BaseEnemy<EggheadConfig, EggheadComponents>;

    public class Egghead : EggheadType, IStompable
    {
        protected override void InitializeStateMachine()
        {
            var idleState = new CommonIdleState<EggheadType>();
            var patrolState = new CommonPatrolState<EggheadType>();
            var attackState = new CommonAttackState<EggheadType>();
            var takeDamageState = new EggheadTakeDamageState();
            var deathState = new CommonDeathState<EggheadType>();

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

            State<EggheadType>[] allStates = { idleState, patrolState, attackState };
            foreach (var state in allStates)
            {
                stateMachine.AddTransition(state, takeDamageState, () => isTakingDamage);
            }

            stateMachine.SetInitialState<CommonIdleState<EggheadType>>();
        }

        public override BoxCollider2D GetAttackCollider() =>
            config.PlayerNearbyCollider; // Egghead doesn't have a separate attack collider

        public override void OnAttackTrigger()
        {
            components.AudioSource.PlayOneShot(config.AttackSound);
            Instantiate(config.BulletPrefab, config.BulletSpawnPoint.position, Quaternion.identity);
        }

        public override void CompleteTakeDamage()
        {
            isTakingDamage = false;
            if (stateMachine.CurrentState is EggheadTakeDamageState takeDamageState)
            {
                takeDamageState.OnTakeDamageComplete();
            }
        }

        public override void OnPlayerAttack()
        {
            stateMachine
                .ChangeState<EggheadTakeDamageState>();
        }

        public new void OnStomped() => OnPlayerAttack();
    }

    public class EggheadTakeDamageState : State<EggheadType>
    {
        public bool DamageAnimationComplete { get; private set; }
        private State<EggheadType> stateToReturnTo;

        public override void Enter()
        {
            DamageAnimationComplete = false;
            owner.TakeDamage();
            if (owner.GetLives() <= 0 && stateMachine.HasState<CommonDeathState<EggheadType>>())
            {
                stateMachine.ChangeState<CommonDeathState<EggheadType>>();
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

        private State<EggheadType> DetermineStateToReturnTo()
        {
            var previousState = stateMachine.PreviousState;
            if (previousState is CommonAttackState<EggheadType>)
            {
                owner.GetAnimator().SetTrigger("takedamage");
                return owner.Config.ReturnToPatrolAfterAttack
                    ? stateMachine.GetState<CommonPatrolState<EggheadType>>()
                    : stateMachine.GetState<CommonIdleState<EggheadType>>();
            }

            if (previousState is EggheadTakeDamageState previousTakeDamage)
            {
                owner.GetAnimator().Play("takedamage", -1, 0);
                return previousTakeDamage.GetStateToReturnTo();
            }

            owner.GetAnimator().SetTrigger("takedamage");
            return previousState;
        }

        public State<EggheadType> GetStateToReturnTo()
        {
            return stateToReturnTo;
        }
    }

    [System.Serializable]
    public class EggheadConfig : EnemyConfig
    {
        public Transform BulletSpawnPoint;
        public GameObject BulletPrefab;
        public bool ReturnToPatrolAfterAttack = true;
    }

    [System.Serializable]
    public class EggheadComponents : EnemyComponents
    {
    }
}