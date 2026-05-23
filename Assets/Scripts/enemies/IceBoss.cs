using System.Collections;
using System.Collections.Generic;
using enemies;
using UnityEngine;
using System.Linq;

namespace level4
{
    using IceBossType = BaseEnemy<IceBossConfig, IceBossComponents>;

    public class IceBoss : IceBossType
    {
        [HideInInspector] public IceBossState state;

        // Animation hashes
        public static readonly int GroundSlam = Animator.StringToHash("groundslam");
        public static readonly int Punch = Animator.StringToHash("punch");
        public static readonly int Kick = Animator.StringToHash("lightkick");
        public static readonly int Spawn = Animator.StringToHash("spawn");

        private float lastDamageTime = -1f;

        [HideInInspector] public SpriteRenderer spriteRenderer;
        [HideInInspector] public Color originalColor;
        private bool isFullyOnScreen = false;

        private bool _isCenterOnScreen = false;
        private float _centerOnScreenTime = 0f;
        private const float ATTACKABLE_DELAY = 1f;
        private bool _hasBeenAttackable = false;

        private bool IsCenterOnScreen()
        {
            Vector3 viewportPosition = Camera.main.WorldToViewportPoint(transform.position);
            bool centerOnScreen = viewportPosition.x >= 0 && viewportPosition.x <= 1 &&
                                  viewportPosition.y >= 0 && viewportPosition.y <= 1;

            return centerOnScreen;
        }

        protected override void InitializeStateMachine()
        {
            moveSpeed += BossSpawnManager.Instance.bossesSpawned;
            if (PersistentData.Instance != null && PersistentData.Instance.DebugInfiniteLife)
                lives = 1;
            else
                lives += 2 * BossSpawnManager.Instance.bossesSpawned;
            BossSpawnManager.Instance.bossHealthBar.InitializeHealthBars(lives, PersistentData.Instance.CurrentLevelConfig.BossHealthBarPrefab);

            state = new IceBossState();

            var idleState = new CommonIdleState<IceBossType>();
            var chasePlayerState = new CommonChasePlayerState<IceBossType>();
            var attack1State = new IceAttack1State();
            var attack2State = new IceAttack2State();
            var attack3State = new IceAttack3State();
            var spawnState = new IceSpawnState();
            var takeDamageState = new IceTakeDamageState();
            var deathState = new CommonDeathState<IceBossType>();

            stateMachine.AddState(idleState);
            stateMachine.AddState(chasePlayerState);
            stateMachine.AddState(attack1State);
            stateMachine.AddState(attack2State);
            stateMachine.AddState(attack3State);
            stateMachine.AddState(spawnState);
            stateMachine.AddState(takeDamageState);
            stateMachine.AddState(deathState);

            // Add transitions
            stateMachine.AddTransition(idleState, chasePlayerState, () => idleState.IdleTimeElapsed);
            stateMachine.AddTransition(chasePlayerState, () =>
            {
                if (chasePlayerState.PlayerNearby)
                {
                    int attackInt = BossSpawnManager.GetDifficultyInfluencedInt(new float[3] { .7f, .8f, 1f });
                    if (attackInt == 1)
                        return attack1State;
                    if (attackInt == 2)
                        return attack2State;
                    if (attackInt == 3)
                        return attack3State;
                }

                return null;
            });

            stateMachine.AddTransition(attack1State, () =>
            {
                if (attack1State.AttackComplete)
                    return BossSpawnManager.GetDifficultyInfluencedInt(new float[2] { .8f, 1f }) > 1 ? attack2State : idleState;

                return null;
            });

            stateMachine.AddTransition(attack2State, () =>
            {
                if (attack2State.AttackComplete)
                    return BossSpawnManager.GetDifficultyInfluencedInt(new float[2] { .8f, 1f }) > 1 ? attack3State : idleState;

                return null;
            });

            stateMachine.AddTransition(attack3State, () => { return attack3State.AttackComplete ? idleState : null; });


            stateMachine.AddTransition(takeDamageState, () =>
            {
                if (takeDamageState.DamageAnimationComplete)
                {
                    var previousState = takeDamageState.GetPreviousState();
                    if (previousState is IceSpawnState spawnState && !spawnState.SpawnComplete)
                    {
                        return spawnState.GetNextState();
                    }

                    return previousState;
                }

                return null;
            });

            stateMachine.AddTransition(spawnState, () =>
            {
                if (spawnState.SpawnComplete)
                {
                    return spawnState.GetNextState();
                }

                return null;
            });

            deathState.OnStateEnter += () =>
            {
                Player.Instance.OnBossStartToDeathAnim();
            };

            stateMachine.SetInitialState<CommonIdleState<IceBossType>>();

            spriteRenderer = GetComponent<SpriteRenderer>();
            originalColor = spriteRenderer.color;
        }

        protected override void FixedUpdate()
        {
            if (!player.gameObject.activeInHierarchy) return;
            stateMachine.Update();

            if (!_hasBeenAttackable)
            {
                bool currentCenterOnScreen = IsCenterOnScreen();
                if (currentCenterOnScreen != _isCenterOnScreen)
                {
                    _isCenterOnScreen = currentCenterOnScreen;
                    if (_isCenterOnScreen)
                    {
                        _centerOnScreenTime = Time.time;
                    }
                }

                if (_isCenterOnScreen && !isFullyOnScreen && Time.time - _centerOnScreenTime >= ATTACKABLE_DELAY)
                {
                    isFullyOnScreen = true;
                    _hasBeenAttackable = true;
                }
            }
        }

        public override void OnPlayerAttack()
        {
            if (!isFullyOnScreen || !_hasBeenAttackable)
            {
                return;
            }

            if (Time.time - lastDamageTime < config.DamageCooldown)
            {
                return;
            }

            if (stateMachine.CurrentState is IceTakeDamageState)
            {
                return;
            }

            lastDamageTime = Time.time;

            BossSpawnManager.Instance.bossHealthBar.DecreaseHealthBar(1);
            TakeDamage();
            if (lives <= 0)
            {
                stateMachine.ChangeState<IceTakeDamageState>();
                return;
            }
            StartCoroutine(Co_HurtFlash());
            return;
        }

        public override void CompleteTakeDamage()
        {
            isTakingDamage = false;
            if (stateMachine.CurrentState is IceTakeDamageState takeDamageState)
            {
                takeDamageState.OnTakeDamageComplete();
            }
        }

        public void Anim_OnAttackAnimEnd()
        {
            stateMachine.CurrentState.EndState();
        }

        public void Anim_OnSwipeTrigger()
        {
            if (IsPlayerHit(config.LightAttackCollider))
            {
                player.TakeDamage(2);
            }
        }

        public void Anim_OnBiteTrigger()
        {
            if (IsPlayerHit(config.MediumAttackCollider))
            {
                player.TakeDamage(2);
            }
        }

        public void Anim_OnFireBreathTrigger()
        {
            if (IsPlayerHit(config.HeavyAttackCollider))
            {
                player.TakeDamage(2);
            }
        }

        public void OnSpawnEnd()
        {
            if (stateMachine.CurrentState is IceSpawnState spawnState)
            {
                spawnState.OnSpawnEnd();
            }
        }

        public override void OnDeathAnimationEnd()
        {
            base.OnDeathAnimationEnd();
            BossSpawnManager.OnBossDefeated?.Invoke();
        }
    }

    public class IceAttack1State : State<IceBossType>
    {
        public bool AttackComplete { get; private set; }

        public override void Enter()
        {
            AttackComplete = false;
            float direction = owner.Player.transform.position.x < owner.transform.position.x ? -1 : 1;
            owner.transform.rotation = Quaternion.Euler(0, direction == -1 ? 0 : 180, 0);
            owner.Components.Animator.SetTrigger(IceBoss.Kick);
            (owner as IceBoss).state.Attack1Count++;

            owner.SetDirection(direction);
        }

        public override void EndState()
        {
            IceBoss IceBoss = owner as IceBoss;

            AttackComplete = true;

            float direction = owner.Player.transform.position.x < owner.transform.position.x ? -1 : 1;
            owner.transform.rotation = Quaternion.Euler(0, direction == -1 ? 0 : 180, 0);

            owner.SetDirection(direction);
        }

        public override void Exit()
        {
            AttackComplete = false;
        }
    }

    public class IceAttack2State : State<IceBossType>
    {
        public bool AttackComplete { get; private set; }

        public override void Enter()
        {
            float direction = owner.Player.transform.position.x < owner.transform.position.x ? -1 : 1;
            owner.transform.rotation = Quaternion.Euler(0, direction == -1 ? 0 : 180, 0);
            owner.Components.Animator.SetTrigger(IceBoss.Punch);

            owner.SetDirection(direction);
        }

        public override void Update()
        {
        }

        public override void EndState()
        {
            IceBoss IceBoss = owner as IceBoss;

            AttackComplete = true;

            float direction = owner.Player.transform.position.x < owner.transform.position.x ? -1 : 1;
            owner.transform.rotation = Quaternion.Euler(0, direction == -1 ? 0 : 180, 0);

            owner.SetDirection(direction);
        }

        public override void Exit()
        {
            AttackComplete = false;
        }
    }

    public class IceAttack3State : State<IceBossType>
    {
        public bool AttackComplete { get; private set; }

        public override void Enter()
        {
            float direction = owner.Player.transform.position.x < owner.transform.position.x ? -1 : 1;
            owner.transform.rotation = Quaternion.Euler(0, direction == -1 ? 0 : 180, 0);
            owner.Components.Animator.SetTrigger(IceBoss.GroundSlam);

            owner.SetDirection(direction);
        }

        public override void Update()
        {
        }

        public override void EndState()
        {
            IceBoss IceBoss = owner as IceBoss;

            AttackComplete = true;

            float direction = owner.Player.transform.position.x < owner.transform.position.x ? -1 : 1;
            owner.transform.rotation = Quaternion.Euler(0, direction == -1 ? 0 : 180, 0);
            owner.Components.Animator.Play(IceBoss.Punch, -1, 0);

            owner.SetDirection(direction);
        }

        public override void Exit()
        {
            AttackComplete = false;
        }
    }

    public class IceSpawnState : State<IceBossType>
    {
        public bool SpawnComplete { get; private set; }
        private State<IceBossType> _nextState;

        public override void Enter()
        {
            _nextState = stateMachine.PreviousState;

            float direction = owner.Player.transform.position.x < owner.transform.position.x ? -1 : 1;
            owner.transform.rotation = Quaternion.Euler(0, direction == -1 ? 0 : 180, 0);
            owner.Components.Animator.SetTrigger(IceBoss.Spawn);

            owner.SetDirection(direction);
        }

        public State<IceBossType> GetNextState()
        {
            return _nextState;
        }

        public void OnSpawnEnd()
        {
            SpawnComplete = true;
        }

        public override void Exit()
        {
            SpawnComplete = false;
            (owner as IceBoss).state.ShouldSpawn = false;
        }
    }

    public class IceTakeDamageState : CommonTakeDamageState<IceBossType>
    {
        private float _elapsedTime;

        public override void Enter()
        {
            DamageAnimationComplete = false;

            owner.TakeDamage();
            if (owner.GetLives() <= 0 && stateMachine.HasState<CommonDeathState<IceBossType>>())
            {
                stateMachine.ChangeState<CommonDeathState<IceBossType>>();
                return;
            }

            owner.GetAnimator().SetTrigger("takedamage");
            PreviousState = stateMachine.PreviousState;

            AudioSource audioSource = owner.GetAudioSource();
            AudioClip takeDamageSound = owner.GetTakeDamageSound();
            if (audioSource != null && takeDamageSound != null)
            {
                owner.GetAudioSource().PlayOneShot(takeDamageSound);
            }

            _elapsedTime = 0f;
            IceBoss IceBoss = owner as IceBoss;
            IceBoss.spriteRenderer.color = IceBoss.Config.DamageFlashColor;
        }

        public override void Update()
        {
            base.Update();

            IceBoss IceBoss = owner as IceBoss;
            float t = _elapsedTime / IceBoss.Config.FlashDuration;
            IceBoss.spriteRenderer.color = Color.Lerp(IceBoss.Config.DamageFlashColor, IceBoss.originalColor, t);
            _elapsedTime += Time.deltaTime;
        }

        public override void Exit()
        {
            base.Exit();

            IceBoss mechBoss = owner as IceBoss;
            mechBoss.spriteRenderer.color = mechBoss.originalColor;
        }
    }

    [System.Serializable]
    public class IceBossConfig : EnemyConfig
    {
        public BoxCollider2D LightAttackCollider;
        public BoxCollider2D MediumAttackCollider;
        public BoxCollider2D HeavyAttackCollider;

        public float DamageCooldown = 0.2f;
        public Color DamageFlashColor = Color.red;
        public float FlashDuration = 0.15f;
    }

    [System.Serializable]
    public class IceBossComponents : EnemyComponents
    {
    }

    public class IceBossState
    {
        public bool ShouldSpawn { get; set; }
        public int Attack1Count { get; set; }
        public int RemainingAttacks { get; set; }

        public IceBossState()
        {
            ShouldSpawn = false;
            Attack1Count = 0;
            RemainingAttacks = 0;
        }
    }
}
