using System.Collections;
using System.Collections.Generic;
using enemies;
using UnityEngine;

namespace level2
{
    using MechBossType = BaseEnemy<MechBossConfig, MechBossComponents>;

    public class MechBoss : MechBossType
    {
        [HideInInspector] public MechBossState state;
        private Coroutine _spawnSlimeCoroutine;

        // Animation hashes
        public static readonly int LightAttack = Animator.StringToHash("lightattack");
        public static readonly int GrabPrepare = Animator.StringToHash("grabprepare");
        public static readonly int GrabSuccess = Animator.StringToHash("grabsuccess");
        public static readonly int GrabFail = Animator.StringToHash("grabfail");
        public static readonly int Spawn = Animator.StringToHash("spawn");

        private float lastDamageTime = -1f;

        [HideInInspector] public SpriteRenderer spriteRenderer;
        [HideInInspector] public Color originalColor;
        private bool isFullyOnScreen = false;

        private bool _hasBeenAttackable = false;
        List<GameObject> spawnedSlimes = new();

        protected override void InitializeStateMachine()
        {
            moveSpeed += BossSpawnManager.Instance.bossesSpawned;
            if (PersistentData.Instance != null && PersistentData.Instance.DebugInfiniteLife)
                lives = 1;
            else
                lives += 2 * BossSpawnManager.Instance.bossesSpawned;
            BossSpawnManager.Instance.bossHealthBar.InitializeHealthBars(lives, PersistentData.Instance.CurrentLevelConfig.BossHealthBarPrefab);

            state = new MechBossState();

            var idleState = new CommonIdleState<MechBossType>();
            var chasePlayerState = new CommonChasePlayerState<MechBossType>();
            var attack1State = new MechAttack1State();
            var attack2PrepareState = new MechAttack2PrepareState();
            var attack2SuccessState = new MechAttack2SuccessState();
            var attack2FailState = new MechAttack2FailState();
            var spawnState = new MechSpawnState();
            var takeDamageState = new MechTakeDamageState();
            var deathState = new CommonDeathState<MechBossType>();

            stateMachine.AddState(idleState);
            stateMachine.AddState(chasePlayerState);
            stateMachine.AddState(attack1State);
            stateMachine.AddState(attack2PrepareState);
            stateMachine.AddState(attack2SuccessState);
            stateMachine.AddState(attack2FailState);
            stateMachine.AddState(spawnState);
            stateMachine.AddState(takeDamageState);
            stateMachine.AddState(deathState);

            // Add transitions
            stateMachine.AddTransition(idleState, chasePlayerState, () => idleState.IdleTimeElapsed);
            stateMachine.AddTransition(chasePlayerState, () =>
            {
                if (chasePlayerState.PlayerNearby)
                {
                    return attack1State;
                }

                return null;
            });

            stateMachine.AddTransition(attack1State, () =>
            {
                if (attack1State.AttackComplete)
                {
                    if (state.Attack1Count >= 2)
                    {
                        state.Attack1Count = 0;
                        return attack2PrepareState;
                    }

                    return chasePlayerState;
                }

                return null;
            });

            stateMachine.AddTransition(attack2PrepareState, attack2SuccessState,
                () => attack2PrepareState.PrepareComplete && attack2PrepareState.PlayerGrabbed);
            stateMachine.AddTransition(attack2PrepareState, attack2FailState,
                () => attack2PrepareState.PrepareComplete && !attack2PrepareState.PlayerGrabbed);
            stateMachine.AddTransition(attack2FailState, chasePlayerState, () => attack2FailState.FailComplete);

            State<MechBossType>[] attackableStates =
                { idleState, chasePlayerState, attack1State, attack2PrepareState, attack2FailState };
            foreach (var aState in attackableStates)
            {
                stateMachine.AddTransition(aState, spawnState, () => state.ShouldSpawn);
            }

            stateMachine.AddTransition(takeDamageState, () =>
            {
                if (takeDamageState.DamageAnimationComplete)
                {
                    var previousState = takeDamageState.GetPreviousState();
                    if (previousState is MechSpawnState lSpawnState && !lSpawnState.SpawnComplete)
                    {
                        return lSpawnState.GetNextState();
                    }

                    if (previousState is MechAttack2PrepareState)
                    {
                        return chasePlayerState;
                    }

                    if (previousState is MechAttack2FailState)
                    {
                        return chasePlayerState;
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
                foreach(var obj in spawnedSlimes)
                {
                    if (obj && obj.activeInHierarchy && obj.TryGetComponent(out BlueSlime enemy))
                    {
                        enemy.Die();
                    }
                }
            };

            stateMachine.SetInitialState<CommonIdleState<MechBossType>>();

            _spawnSlimeCoroutine = StartCoroutine(SpawnSlimeRoutine());

            spriteRenderer = GetComponent<SpriteRenderer>();
            originalColor = spriteRenderer.color;
        }

        protected override void FixedUpdate()
        {
            if (!player.gameObject.activeInHierarchy) return;
            stateMachine.Update();

            if (!_hasBeenAttackable)
            {
                if (!isFullyOnScreen)
                {
                    isFullyOnScreen = true;
                    _hasBeenAttackable = true;
                }
            }
        }

        private IEnumerator SpawnSlimeRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(config.SlimeSpawnInterval);

                // Wait until not attacking
                yield return new WaitUntil(() => stateMachine.CurrentState is not MechAttack1State &&
                                                 stateMachine.CurrentState is not MechAttack2PrepareState &&
                                                 stateMachine.CurrentState is not MechAttack2SuccessState &&
                                                 stateMachine.CurrentState is not MechAttack2FailState);
                state.ShouldSpawn = true;
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
            
            if (stateMachine.CurrentState is MechTakeDamageState)
            {
                return;
            }

            lastDamageTime = Time.time;

            BossSpawnManager.Instance.bossHealthBar.DecreaseHealthBar(1);
            TakeDamage();
            StartCoroutine(Co_HurtFlash());
            if (lives <= 0)
            {
                stateMachine.ChangeState<MechTakeDamageState>();
                return;
            }
        }

        public override void CompleteTakeDamage()
        {
            isTakingDamage = false;
            if (stateMachine.CurrentState is MechTakeDamageState takeDamageState)
            {
                takeDamageState.OnTakeDamageComplete();
            }
        }

        public void OnLightAttackTrigger()
        {
            components.Rigidbody.linearVelocity = Vector2.zero;
            if (IsPlayerHit(config.LightAttackCollider))
            {
                player.TakeDamage(2);
            }
        }

        public void OnLightAttackEnd()
        {
            if (stateMachine.CurrentState is MechAttack1State attackState)
            {
                attackState.OnAttackEnd();
            }
        }

        public void OnGrabPrepareEnd()
        {
            if (stateMachine.CurrentState is MechAttack2PrepareState attackState)
            {
                attackState.OnPrepareEnd();
            }
        }

        public void OnGrabSuccessTrigger()
        {
        }

        public void OnGrabSuccessEnd()
        {
            player.TakeDamage(1000); // Instant death
        }

        public void OnGrabFailEnd()
        {
            if (stateMachine.CurrentState is MechAttack2FailState attackState)
            {
                attackState.OnFailComplete();
            }
        }

        public void OnSpawnTrigger()
        {
            GameObject instantiated = Instantiate(config.BlueSlimePrefab, config.SlimeSpawnPoint.position,
                config.SlimeSpawnPoint.rotation, transform.parent); // Child of boss pattern
            spawnedSlimes.Add(instantiated);
        }

        public void OnSpawnEnd()
        {
            if (stateMachine.CurrentState is MechSpawnState spawnState)
            {
                spawnState.OnSpawnEnd();
            }
        }

        public override void OnDeathAnimationEnd()
        {
            base.OnDeathAnimationEnd();
            BossSpawnManager.OnBossDefeated?.Invoke();
        }

        private void OnDestroy()
        {
            if (_spawnSlimeCoroutine != null)
            {
                StopCoroutine(_spawnSlimeCoroutine);
            }
        }

    }

    public class MechAttack1State : State<MechBossType>
    {
        public bool AttackComplete { get; private set; }
        float timer = 0;

        public override void Enter()
        {
            timer = 0;
            AttackComplete = false;
            float direction = owner.Player.transform.position.x < owner.transform.position.x ? -1 : 1;
            owner.transform.rotation = Quaternion.Euler(0, direction == -1 ? 0 : 180, 0);
            owner.Components.Animator.SetTrigger(MechBoss.LightAttack);
            (owner as MechBoss).state.Attack1Count++;
            owner.SetDirection(direction);

            if ((Player.Instance.transform.position - owner.transform.position).magnitude < 6f)
            {
                float direction2 = Camera.main.transform.position.x - owner.transform.position.x < 0 ? 1 : -1;
                owner.SetDirection(direction2);
                Vector2 velocity = owner.transform.right * 20f;
                owner.Components.Rigidbody.linearVelocity = velocity;
            }
        }

        public override void Update()
        {
            timer += Time.deltaTime;
            if (timer > 2f)
            {
                owner.Components.Rigidbody.linearVelocity = Vector2.zero;
                return;
            }
            owner.Components.Rigidbody.linearVelocity = Vector2.Lerp(owner.Components.Rigidbody.linearVelocity, Vector2.zero, timer / 2f);
        }

        public void OnAttackEnd()
        {
            MechBoss mechBoss = owner as MechBoss;
            mechBoss.state.RemainingAttacks--;
            if (mechBoss.state.RemainingAttacks <= 0)
            {
                AttackComplete = true;
            }
            else
            {
                float direction = owner.Player.transform.position.x < owner.transform.position.x ? -1 : 1;
                owner.transform.rotation = Quaternion.Euler(0, direction == -1 ? 0 : 180, 0);
                owner.Components.Animator.Play(MechBoss.LightAttack, -1, 0);
                
                owner.SetDirection(direction);
            }
        }

        public override void Exit()
        {
            AttackComplete = false;
        }
    }

    public class MechAttack2PrepareState : State<MechBossType>
    {
        public bool PlayerGrabbed { get; private set; }
        public bool PrepareComplete { get; private set; }
        float timer = 0;

        public override void Enter()
        {
            timer = 0;
            PlayerGrabbed = false;
            PrepareComplete = false;
            float direction = owner.Player.transform.position.x < owner.transform.position.x ? -1 : 1;
            owner.transform.rotation = Quaternion.Euler(0, direction == -1 ? 0 : 180, 0);
            owner.Components.Animator.SetTrigger(MechBoss.GrabPrepare);
            owner.SetDirection(direction);

            if ((Player.Instance.transform.position - owner.transform.position).magnitude < 6f)
            {
                float direction2 = Camera.main.transform.position.x - owner.transform.position.x < 0 ? 1 : -1;
                owner.transform.rotation = Quaternion.Euler(0, direction2 == -1 ? 0 : 180, 0);
                owner.SetDirection(direction2);
                Vector2 velocity = owner.transform.right * 20f;
                owner.Components.Rigidbody.linearVelocity = velocity;
            }

        }

        public override void Update()
        {
            timer += Time.deltaTime;
            if (timer > 2f)
            {
                owner.Components.Rigidbody.linearVelocity = Vector2.zero;
                return;
            }
            owner.Components.Rigidbody.linearVelocity = Vector2.Lerp(owner.Components.Rigidbody.linearVelocity, Vector2.zero, timer / 2f);
        }

        public void OnPrepareEnd()
        {
            PlayerGrabbed = owner.IsPlayerHit(owner.Config.GrabCollider);
            PrepareComplete = true;
        }

        public override void Exit()
        {
        }
    }

    public class MechAttack2SuccessState : State<MechBossType>
    {
        public override void Enter()
        {
            owner.Player.DisableControlsAndColliders = true;
            owner.Player.SpriteRenderer.enabled = false;
            owner.Components.Animator.SetTrigger(MechBoss.GrabSuccess);
        }

        public override void Exit()
        {
        }
    }

    public class MechAttack2FailState : State<MechBossType>
    {
        public bool FailComplete { get; private set; }

        public override void Enter()
        {
            FailComplete = false;
            owner.Components.Animator.SetTrigger(MechBoss.GrabFail);
        }

        public void OnFailComplete()
        {
            FailComplete = true;
        }

        public override void Exit()
        {
            FailComplete = false;
        }
    }

    public class MechSpawnState : State<MechBossType>
    {
        public bool SpawnComplete { get; private set; }
        private State<MechBossType> _nextState;

        public override void Enter()
        {
            _nextState = stateMachine.PreviousState;

            float direction = owner.Player.transform.position.x < owner.transform.position.x ? -1 : 1;
            owner.transform.rotation = Quaternion.Euler(0, direction == -1 ? 0 : 180, 0);
            owner.Components.Animator.SetTrigger(MechBoss.Spawn);
            
            owner.SetDirection(direction);
        }

        public State<MechBossType> GetNextState()
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
            (owner as MechBoss).state.ShouldSpawn = false;
        }
    }

    public class MechTakeDamageState : CommonTakeDamageState<MechBossType>
    {
        private float _elapsedTime;

        public override void Enter()
        {
            owner.Components.Rigidbody.linearVelocity = Vector3.zero;
            DamageAnimationComplete = false;

            owner.TakeDamage();
            if (owner.GetLives() <= 0 && stateMachine.HasState<CommonDeathState<MechBossType>>())
            {
                stateMachine.ChangeState<CommonDeathState<MechBossType>>();
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
            MechBoss mechBoss = owner as MechBoss;
            mechBoss.spriteRenderer.color = mechBoss.Config.DamageFlashColor;
        }

        public override void Update()
        {
            base.Update();

            MechBoss mechBoss = owner as MechBoss;
            float t = _elapsedTime / mechBoss.Config.FlashDuration;
            mechBoss.spriteRenderer.color = Color.Lerp(mechBoss.Config.DamageFlashColor, mechBoss.originalColor, t);
            _elapsedTime += Time.deltaTime;
        }

        public override void Exit()
        {
            base.Exit();

            MechBoss mechBoss = owner as MechBoss;
            mechBoss.spriteRenderer.color = mechBoss.originalColor;
        }
    }

    [System.Serializable]
    public class MechBossConfig : EnemyConfig
    {
        public float SlimeSpawnInterval = 10f;

        public BoxCollider2D LightAttackCollider;
        public BoxCollider2D GrabCollider;
        public GameObject BlueSlimePrefab;
        public Transform SlimeSpawnPoint;

        public float DamageCooldown = 0.2f;
        public Color DamageFlashColor = Color.red;
        public float FlashDuration = 0.15f;
    }

    [System.Serializable]
    public class MechBossComponents : EnemyComponents
    {
    }

    public class MechBossState
    {
        public bool ShouldSpawn { get; set; }
        public int Attack1Count { get; set; }
        public int RemainingAttacks { get; set; }

        public MechBossState()
        {
            ShouldSpawn = false;
            Attack1Count = 0;
            RemainingAttacks = 0;
        }
    }
}
