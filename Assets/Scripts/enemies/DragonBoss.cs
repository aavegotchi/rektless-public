using System.Collections;
using enemies;
using UnityEngine;

namespace level2
{
    using DragonBossType = BaseEnemy<DragonBossConfig, DragonBossComponents>;

    public class DragonBoss : DragonBossType
    {
        [HideInInspector] public DragonBossState state;

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

            state = new DragonBossState();

            var idleState = new CommonIdleState<DragonBossType>();
            var chasePlayerState = new CommonChasePlayerState<DragonBossType>();
            var attack1State = new DragonAttack1State();
            var attack2State = new DragonAttack2State();
            var attack3State = new DragonAttack3State();
            var spawnState = new DragonSpawnState();
            var takeDamageState = new DragonTakeDamageState();
            var deathState = new CommonDeathState<DragonBossType>();

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
                    if (previousState is DragonSpawnState spawnState && !spawnState.SpawnComplete)
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

            stateMachine.SetInitialState<CommonIdleState<DragonBossType>>();

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

            if (stateMachine.CurrentState is DragonTakeDamageState)
            {
                return;
            }

            lastDamageTime = Time.time;

            BossSpawnManager.Instance.bossHealthBar.DecreaseHealthBar(1);
            TakeDamage();
            if (lives <= 0)
            {
                stateMachine.ChangeState<DragonTakeDamageState>();
                return;
            }
            StartCoroutine(Co_HurtFlash());


            //stateMachine.ChangeState<DragonTakeDamageState>();
        }

        public override void CompleteTakeDamage()
        {
            isTakingDamage = false;
            if (stateMachine.CurrentState is DragonTakeDamageState takeDamageState)
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
            if (IsPlayerHit(config.SwipeCollider))
            {
                player.TakeDamage(2);
            }
        }

        public void Anim_OnBiteTrigger()
        {
            if (IsPlayerHit(config.BiteCollider))
            {
                player.TakeDamage(2);
            }
        }

        public void Anim_OnFireBreathTrigger()
        {
            if (IsPlayerHit(config.FireBreathCollider))
            {
                player.TakeDamage(2);
            }
        }

        public void OnSpawnEnd()
        {
            if (stateMachine.CurrentState is DragonSpawnState spawnState)
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

    public class DragonAttack1State : State<DragonBossType>
    {
        public bool AttackComplete { get; private set; }

        public override void Enter()
        {
            owner.PlayAudioClip(owner.Config.attack1SFX);
            AttackComplete = false;
            float direction = owner.Player.transform.position.x < owner.transform.position.x ? -1 : 1;
            owner.transform.rotation = Quaternion.Euler(0, direction == -1 ? 0 : 180, 0);
            owner.Components.Animator.SetTrigger(DragonBoss.Kick);
            (owner as DragonBoss).state.Attack1Count++;

            owner.SetDirection(direction);
        }

        public override void EndState()
        {
            DragonBoss DragonBoss = owner as DragonBoss;

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

    public class DragonAttack2State : State<DragonBossType>
    {
        public bool AttackComplete { get; private set; }

        public override void Enter()
        {
            owner.PlayAudioClip(owner.Config.attack2SFX);
            float direction = owner.Player.transform.position.x < owner.transform.position.x ? -1 : 1;
            owner.transform.rotation = Quaternion.Euler(0, direction == -1 ? 0 : 180, 0);
            owner.Components.Animator.SetTrigger(DragonBoss.Punch);

            owner.SetDirection(direction);
        }

        public override void Update()
        {
        }

        public override void EndState()
        {
            DragonBoss DragonBoss = owner as DragonBoss;

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

    public class DragonAttack3State : State<DragonBossType>
    {
        public bool AttackComplete { get; private set; }

        public override void Enter()
        {
            owner.PlayAudioClip(owner.Config.attack3SFX);
            float direction = owner.Player.transform.position.x < owner.transform.position.x ? -1 : 1;
            owner.transform.rotation = Quaternion.Euler(0, direction == -1 ? 0 : 180, 0);
            owner.Components.Animator.SetTrigger(DragonBoss.GroundSlam);

            owner.SetDirection(direction);
        }

        public override void Update()
        {
        }

        public override void EndState()
        {
            DragonBoss DragonBoss = owner as DragonBoss;

            AttackComplete = true;

            float direction = owner.Player.transform.position.x < owner.transform.position.x ? -1 : 1;
            owner.transform.rotation = Quaternion.Euler(0, direction == -1 ? 0 : 180, 0);
            owner.Components.Animator.Play(DragonBoss.Punch, -1, 0);

            owner.SetDirection(direction);
        }

        public override void Exit()
        {
            AttackComplete = false;
        }
    }

    public class DragonSpawnState : State<DragonBossType>
    {
        public bool SpawnComplete { get; private set; }
        private State<DragonBossType> _nextState;

        public override void Enter()
        {
            _nextState = stateMachine.PreviousState;

            float direction = owner.Player.transform.position.x < owner.transform.position.x ? -1 : 1;
            owner.transform.rotation = Quaternion.Euler(0, direction == -1 ? 0 : 180, 0);
            owner.Components.Animator.SetTrigger(DragonBoss.Spawn);

            owner.SetDirection(direction);
        }

        public State<DragonBossType> GetNextState()
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
            (owner as DragonBoss).state.ShouldSpawn = false;
        }
    }

    public class DragonTakeDamageState : CommonTakeDamageState<DragonBossType>
    {
        private float _elapsedTime;

        public override void Enter()
        {

            DamageAnimationComplete = false;

            owner.TakeDamage();
            if (owner.GetLives() <= 0 && stateMachine.HasState<CommonDeathState<DragonBossType>>())
            {
                stateMachine.ChangeState<CommonDeathState<DragonBossType>>();
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
            DragonBoss DragonBoss = owner as DragonBoss;
            DragonBoss.spriteRenderer.color = DragonBoss.Config.DamageFlashColor;
        }

        public override void Update()
        {
            base.Update();

            DragonBoss DragonBoss = owner as DragonBoss;
            float t = _elapsedTime / DragonBoss.Config.FlashDuration;
            DragonBoss.spriteRenderer.color = Color.Lerp(DragonBoss.Config.DamageFlashColor, DragonBoss.originalColor, t);
            _elapsedTime += Time.deltaTime;
        }

        public override void Exit()
        {
            base.Exit();

            DragonBoss mechBoss = owner as DragonBoss;
            mechBoss.spriteRenderer.color = mechBoss.originalColor;
        }
    }

    [System.Serializable]
    public class DragonBossConfig : EnemyConfig
    {
        public BoxCollider2D SwipeCollider;
        public BoxCollider2D BiteCollider;
        public BoxCollider2D FireBreathCollider;

        public float DamageCooldown = 0.2f;
        public Color DamageFlashColor = Color.red;
        public float FlashDuration = 0.15f;

        [Header("Audio")]
        public AudioClip attack1SFX;
        public AudioClip attack2SFX;
        public AudioClip attack3SFX;
        public AudioClip walkSFX;
    }

    [System.Serializable]
    public class DragonBossComponents : EnemyComponents
    {
    }

    public class DragonBossState
    {
        public bool ShouldSpawn { get; set; }
        public int Attack1Count { get; set; }
        public int RemainingAttacks { get; set; }

        public DragonBossState()
        {
            ShouldSpawn = false;
            Attack1Count = 0;
            RemainingAttacks = 0;
        }
    }
}
