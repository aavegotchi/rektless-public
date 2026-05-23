using System.Collections;
using enemies;
using UnityEngine;

namespace level2
{
    using TreeBossType = BaseEnemy<TreeBossConfig, TreeBossComponents>;

    public class TreeBoss : TreeBossType
    {
        [HideInInspector] public TreeBossState state;

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
            config.MoveSpeed += BossSpawnManager.Instance.bossesSpawned;
            if (PersistentData.Instance != null && PersistentData.Instance.DebugInfiniteLife)
                lives = 1;
            else
                lives += 2 * BossSpawnManager.Instance.bossesSpawned;
            BossSpawnManager.Instance.bossHealthBar.InitializeHealthBars(lives, PersistentData.Instance.CurrentLevelConfig.BossHealthBarPrefab);

            state = new TreeBossState();

            var idleState = new CommonIdleState<TreeBossType>();
            var chasePlayerState = new CommonChasePlayerState<TreeBossType>();
            var attack1State = new TreeAttack1State();
            var attack2State = new TreeAttack2State();
            var attack3State = new TreeAttack3State();
            var spawnState = new TreeSpawnState();
            var takeDamageState = new TreeTakeDamageState();
            var deathState = new CommonDeathState<TreeBossType>();

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

            stateMachine.AddTransition(attack3State, () => { return attack3State.AttackComplete ? idleState :  null; });


            stateMachine.AddTransition(takeDamageState, () =>
            {
                if (takeDamageState.DamageAnimationComplete)
                {
                    var previousState = takeDamageState.GetPreviousState();
                    if (previousState is TreeSpawnState spawnState && !spawnState.SpawnComplete)
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

            stateMachine.SetInitialState<CommonIdleState<TreeBossType>>();

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
            
            if (stateMachine.CurrentState is TreeTakeDamageState)
            {
                return;
            }

            lastDamageTime = Time.time;

            BossSpawnManager.Instance.bossHealthBar.DecreaseHealthBar(1);
            TakeDamage();
            if (lives <= 0)
            {
                stateMachine.ChangeState<TreeTakeDamageState>();
                return;
            }
            StartCoroutine(Co_HurtFlash());

        }

        public override void CompleteTakeDamage()
        {
            isTakingDamage = false;
            if (stateMachine.CurrentState is TreeTakeDamageState takeDamageState)
            {
                takeDamageState.OnTakeDamageComplete();
            }
        }

        public void Anim_OnAttackAnimEnd()
        {
            stateMachine.CurrentState.EndState();
        }

        public void Anim_OnLightKickTrigger()
        {
            if (IsPlayerHit(config.KickCollider))
            {
                player.TakeDamage(2);
            }
        }

        public void Anim_OnPunchTrigger()
        {
            if (IsPlayerHit(config.PunchCollider))
            {
                player.TakeDamage(2);
            }
        }

        public void Anim_OnGroundSlamTrigger()
        {
            if (IsPlayerHit(config.GroundSlamCollider))
            {
                player.TakeDamage(2);
            }
        }

        public void OnSpawnEnd()
        {
            if (stateMachine.CurrentState is TreeSpawnState spawnState)
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

    public class TreeAttack1State : State<TreeBossType>
    {
        public bool AttackComplete { get; private set; }

        public override void Enter()
        {
            owner.PlayAudioClip(owner.Config.attack1SFX);
            AttackComplete = false;
            float direction = owner.Player.transform.position.x < owner.transform.position.x ? -1 : 1;
            owner.transform.rotation = Quaternion.Euler(0, direction == -1 ? 0 : 180, 0);
            owner.Components.Animator.SetTrigger(TreeBoss.Kick);
            (owner as TreeBoss).state.Attack1Count++;
            
            owner.SetDirection(direction);
        }

        public override void EndState()
        {
            TreeBoss treeBoss = owner as TreeBoss;

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

    public class TreeAttack2State : State<TreeBossType>
    {
        public bool AttackComplete { get; private set; }

        public override void Enter()
        {
            owner.PlayAudioClip(owner.Config.attack2SFX);
            float direction = owner.Player.transform.position.x < owner.transform.position.x ? -1 : 1;
            owner.transform.rotation = Quaternion.Euler(0, direction == -1 ? 0 : 180, 0);
            owner.Components.Animator.SetTrigger(TreeBoss.Punch);
            
            owner.SetDirection(direction);
        }

        public override void Update()
        {
        }

        public override void EndState()
        {
            TreeBoss treeBoss = owner as TreeBoss;

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

    public class TreeAttack3State : State<TreeBossType>
    {
        public bool AttackComplete { get; private set; }

        public override void Enter()
        {
            owner.PlayAudioClip(owner.Config.attack3SFX);
            float direction = owner.Player.transform.position.x < owner.transform.position.x ? -1 : 1;
            owner.transform.rotation = Quaternion.Euler(0, direction == -1 ? 0 : 180, 0);
            owner.Components.Animator.SetTrigger(TreeBoss.GroundSlam);

            owner.SetDirection(direction);
        }

        public override void Update()
        {
        }

        public override void EndState()
        {
            TreeBoss treeBoss = owner as TreeBoss;

            AttackComplete = true;

            float direction = owner.Player.transform.position.x < owner.transform.position.x ? -1 : 1;
            owner.transform.rotation = Quaternion.Euler(0, direction == -1 ? 0 : 180, 0);
            owner.Components.Animator.Play(TreeBoss.Punch, -1, 0);

            owner.SetDirection(direction);
        }

        public override void Exit()
        {
            AttackComplete = false;
        }
    }

    public class TreeSpawnState : State<TreeBossType>
    {
        public bool SpawnComplete { get; private set; }
        private State<TreeBossType> _nextState;

        public override void Enter()
        {
            _nextState = stateMachine.PreviousState;

            float direction = owner.Player.transform.position.x < owner.transform.position.x ? -1 : 1;
            owner.transform.rotation = Quaternion.Euler(0, direction == -1 ? 0 : 180, 0);
            owner.Components.Animator.SetTrigger(TreeBoss.Spawn);
            
            owner.SetDirection(direction);
        }

        public State<TreeBossType> GetNextState()
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
            (owner as TreeBoss).state.ShouldSpawn = false;
        }
    }

    public class TreeTakeDamageState : CommonTakeDamageState<TreeBossType>
    {
        private float _elapsedTime;

        public override void Enter()
        {
            DamageAnimationComplete = false;

            owner.TakeDamage();
            if (owner.GetLives() <= 0 && stateMachine.HasState<CommonDeathState<TreeBossType>>())
            {
                stateMachine.ChangeState<CommonDeathState<TreeBossType>>();
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
            TreeBoss treeBoss = owner as TreeBoss;
            treeBoss.spriteRenderer.color = treeBoss.Config.DamageFlashColor;
        }

        public override void Update()
        {
            base.Update();

            TreeBoss treeBoss = owner as TreeBoss;
            float t = _elapsedTime / treeBoss.Config.FlashDuration;
            treeBoss.spriteRenderer.color = Color.Lerp(treeBoss.Config.DamageFlashColor, treeBoss.originalColor, t);
            _elapsedTime += Time.deltaTime;
        }

        public override void Exit()
        {
            base.Exit();

            TreeBoss mechBoss = owner as TreeBoss;
            mechBoss.spriteRenderer.color = mechBoss.originalColor;
        }
    }

    [System.Serializable]
    public class TreeBossConfig : EnemyConfig
    {
        public BoxCollider2D PunchCollider;
        public BoxCollider2D KickCollider;
        public BoxCollider2D GroundSlamCollider;

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
    public class TreeBossComponents : EnemyComponents
    {
    }

    public class TreeBossState
    {
        public bool ShouldSpawn { get; set; }
        public int Attack1Count { get; set; }
        public int RemainingAttacks { get; set; }

        public TreeBossState()
        {
            ShouldSpawn = false;
            Attack1Count = 0;
            RemainingAttacks = 0;
        }
    }
}
