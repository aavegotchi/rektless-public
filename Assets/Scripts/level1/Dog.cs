using enemies;
using UnityEngine;

namespace level2
{
    using DogType = BaseEnemy<DogConfig, DogComponents>;

    public class Dog : DogType, IStompable
    {
        protected override void InitializeStateMachine()
        {
            var idleState = new CommonIdleState<DogType>();
            var patrolState = new CommonPatrolState<DogType>();
            var attackState = new CommonAttackState<DogType>();
            var chasePlayerState = new CommonChasePlayerState<DogType>();
            var takeDamageState = new CommonTakeDamageState<DogType>();
            var deathState = new CommonDeathState<DogType>();

            stateMachine.AddState(idleState);
            stateMachine.AddState(patrolState);
            stateMachine.AddState(attackState);
            stateMachine.AddState(chasePlayerState);
            stateMachine.AddState(takeDamageState);
            stateMachine.AddState(deathState);

            stateMachine.AddTransition(idleState, () =>
            {
                if (idleState.IdleTimeElapsed)
                {
                    var lastState = idleState.GetPreviousState();
                    return lastState is CommonAttackState<DogType> ? chasePlayerState : patrolState;
                }

                return null;
            });

            stateMachine.AddTransition(attackState, () =>
            {
                if (attackState.AttackComplete)
                {
                    var lastState = attackState.GetPreviousState();
                    return lastState is CommonPatrolState<DogType> ? idleState : chasePlayerState;
                }

                return null;
            });

            stateMachine.AddTransition(chasePlayerState, attackState, () => chasePlayerState.PlayerNearby);
            stateMachine.AddTransition(patrolState, attackState, () => patrolState.PlayerNearby);

            stateMachine.AddTransition(takeDamageState, () =>
            {
                if (takeDamageState.DamageAnimationComplete)
                {
                    var previousState = takeDamageState.GetPreviousState();
                    return previousState is CommonPatrolState<DogType> ? attackState : previousState;
                }

                return null;
            });

            State<DogType>[] allStates = { idleState, patrolState, attackState, chasePlayerState };
            foreach (var state in allStates)
            {
                stateMachine.AddTransition(state, takeDamageState, () => isTakingDamage);
            }

            patrolState.OnStateUpdate += () =>
            {
                if (IsPlayerHit(Config.ChargeCollider))
                {
                    Player.Instance.TakeDamage();
                    if (Player.Instance.gameObject.activeInHierarchy)
                    {
                        PushPlayer();
                    }
                }
            };

            moveSpeed = Config.ChaseMoveSpeed;
            patrolState.OnStateEnter += () => SetMoveSpeed(Config.ChargeMoveSpeed);
            chasePlayerState.OnStateEnter += () => SetMoveSpeed(Config.ChaseMoveSpeed);

            stateMachine.SetInitialState<CommonIdleState<DogType>>();
        }

        public override AudioClip GetTakeDamageSound() => config.TakeDamageSound;

        public override void OnAttackTrigger()
        {
            components.AudioSource.PlayOneShot(config.AttackSound);
            if (IsPlayerHit(config.AttackCollider))
            {
                Player.Instance.TakeDamage(1);
                PushPlayer();
            }
        }

        public void PushPlayer()
        {
            Vector2 pushDirection = new Vector2(
                transform.position.x < Player.Instance.transform.position.x ? 1 : -1,
                .5f
            );
            Player.Instance.Push(config.PushForce, pushDirection);
            stateMachine.ChangeState<CommonIdleState<DogType>>();
        }
        
        public new void OnStomped() => OnPlayerAttack();
    }

    [System.Serializable]
    public class DogConfig : EnemyConfig
    {
        public float ChargeMoveSpeed = 8f;
        public float ChaseMoveSpeed = 5f;
        public float PushForce = 5f;
        public Collider2D ChargeCollider;
        public AudioClip TakeDamageSound;
    }

    [System.Serializable]
    public class DogComponents : EnemyComponents
    {
    }
}