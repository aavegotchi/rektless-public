using UnityEngine;
using System.Collections;
using enemies;

namespace level2
{
    using CactusType = BaseEnemy<CactusConfig, CactusComponents>;

    public class Cactus : CactusType, IStompable
    {
        protected override void InitializeStateMachine()
        {
            var idleState = new CommonIdleState<CactusType>();
            var attackState = new CactusAttackState();
            var takeDamageState = new CommonTakeDamageState<CactusType>();
            var deathState = new CommonDeathState<CactusType>();

            stateMachine.AddState(idleState);
            stateMachine.AddState(attackState);
            stateMachine.AddState(takeDamageState);
            stateMachine.AddState(deathState);

            stateMachine.AddTransition(idleState, attackState, () => idleState.IdleTimeElapsed);

            stateMachine.AddTransition(takeDamageState, () =>
            {
                if (takeDamageState.DamageAnimationComplete)
                {
                    return takeDamageState.GetPreviousState();
                }

                return null;
            });

            State<CactusType>[] allStates = { idleState, attackState };
            foreach (var state in allStates)
            {
                stateMachine.AddTransition(state, takeDamageState, () => isTakingDamage);
            }

            stateMachine.SetInitialState<CommonIdleState<CactusType>>();
        }

        public override BoxCollider2D GetPlayerNearbyCollider() => config.PlayerInRangeCollider;

        public override BoxCollider2D GetAttackCollider() =>
            config.PlayerInRangeCollider; // Cactus uses the same collider for attack and player detection

        public void OnSpawnBulletAnimationEvent()
        {
            if (stateMachine.CurrentState is CactusAttackState attackState)
            {
                attackState.OnSpawnBulletAnimationEvent();
            }
        }

        public void OnAttackAnimationEnd()
        {
            if (stateMachine.CurrentState is CactusAttackState attackState)
            {
                attackState.OnAttackAnimationEnd();
            }
        }

        public new void OnStomped() => OnPlayerAttack();
    }

    public class CactusAttackState : State<CactusType>
    {
        private int BulletsSpawned { get; set; }
        private bool PlayerInRange { get; set; }

        public override void Enter()
        {
            owner.GetAnimator().SetTrigger("attack");
            owner.SetDirection(-owner.GetDirection());
            float yRotation = owner.GetDirection() < 0 ? 180 : 0;
            owner.transform.rotation = Quaternion.Euler(0, yRotation, 0);
            PlayerInRange = false;
        }

        public override void Update()
        {
            CheckPlayerInRange();
        }

        private void CheckPlayerInRange()
        {
            var temp = Physics2D.OverlapBox(owner.GetPlayerNearbyCollider().bounds.center,
                owner.GetPlayerNearbyCollider().bounds.size, 0, owner.GetPlayerLayer());
            if (!PlayerInRange && temp)
            {
                owner.GetAnimator().Play("attack", -1, 0);
            }

            PlayerInRange = temp;
        }

        public void OnSpawnBulletAnimationEvent()
        {
            SpawnBullet();
        }

        public void OnAttackAnimationEnd()
        {
            owner.GetAnimator().ResetTrigger("attack");
            owner.StartCoroutine(BulletCooldownCoroutine());
        }

        private IEnumerator BulletCooldownCoroutine()
        {
            yield return new WaitForSeconds(owner.Config.TimeBetweenBullets);
            if (!PlayerInRange || owner.GetLives() <= 0)
            {
                yield break;
            }

            if (BulletsSpawned < owner.Config.BulletsPerAttack)
            {
                owner.transform.rotation = Player.Instance.transform.position.x > owner.transform.position.x
                    ? Quaternion.Euler(0, 180, 0)
                    : Quaternion.identity;
                owner.GetAnimator().Play("attack", -1, 0);
            }
            else
            {
                owner.StartCoroutine(PrepareRestart());
            }
        }

        private void SpawnBullet()
        {
            Quaternion rotation = owner.transform.rotation;
            Object.Instantiate(owner.Config.CactusBulletPrefab, owner.Config.BulletSpawnPoint.position, rotation);

            BulletsSpawned++;
        }

        private IEnumerator PrepareRestart()
        {
            BulletsSpawned = 0;
            yield return new WaitForSeconds(owner.Config.TimeBetweenAttacks);
            if (owner.GetLives() > 0 && PlayerInRange)
            {
                owner.transform.rotation = Player.Instance.transform.position.x > owner.transform.position.x
                    ? Quaternion.Euler(0, 180, 0)
                    : Quaternion.identity;
                owner.GetAnimator().Play("attack", -1, 0);
            }
        }

        public override void Exit()
        {
            owner.GetAnimator().ResetTrigger("attack");
            BulletsSpawned = 0;
        }
    }

    [System.Serializable]
    public class CactusConfig : EnemyConfig
    {
        public int BulletsPerAttack = 3;
        public float TimeBetweenBullets = 0.5f;
        public float TimeBetweenAttacks = 2f;
        public GameObject CactusBulletPrefab;
        public BoxCollider2D PlayerInRangeCollider;
        public Transform BulletSpawnPoint;
    }

    [System.Serializable]
    public class CactusComponents : EnemyComponents
    {
    }
}