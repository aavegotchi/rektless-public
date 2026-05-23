using System;
using System.Collections;
using UnityEngine;

namespace enemies
{
    public abstract class BaseEnemy<TConfig, TComponents> : MonoBehaviour, IEnemy, IAttackable, IBounceable
        where TConfig : EnemyConfig
        where TComponents : EnemyComponents
    {
        [SerializeField] protected TConfig config;
        [SerializeField] protected TComponents components;
        public string currentState;

        protected StateMachine<BaseEnemy<TConfig, TComponents>> stateMachine;
        protected Player player;
        protected int lives;
        protected bool isTakingDamage;
        protected float direction = -1f;
        protected float moveSpeed;
        
        public TConfig Config => config;
        public TComponents Components => components;
        public StateMachine<BaseEnemy<TConfig, TComponents>> StateMachine => stateMachine;
        public Player Player => player;

        float IBounceable.BounceSpeed => 15f;

        bool IBounceable.CanJumpToAddMoreHeight => true;

        protected virtual void Awake()
        {
            lives = config.InitialLives;
            player = Player.Instance;
            SetMoveSpeed(0);
            stateMachine = new StateMachine<BaseEnemy<TConfig, TComponents>>(this);
            AlignToGround();
        }

        protected virtual void Start()
        {
            if (!player.gameObject.activeInHierarchy) return;
            if (!player.OnStarting)
            {
                InitializeStateMachine();
            }
            else
            {
                player.OnStartAction += InitializeStateMachine;
            }
        }

        protected virtual void FixedUpdate()
        {
            if (!player.gameObject.activeInHierarchy) return;
            if (player.OnStarting) return;
            stateMachine.Update();
            currentState = stateMachine.CurrentState.ToString();
        }

        protected abstract void InitializeStateMachine();

        public virtual void Die() => stateMachine.ChangeState<CommonDeathState<BaseEnemy<TConfig, TComponents>>>();
        public virtual Animator GetAnimator() => components.Animator;
        public virtual AudioSource GetAudioSource() => components.AudioSource;
        public virtual Collider2D GetCollider() => components.Collider;
        public virtual LayerMask GetPlayerLayer() => config.PlayerLayer;
        public virtual LayerMask GetSpaceLayer() => config.SpaceLayer;
        public virtual float GetMoveSpeed() => moveSpeed;
        public virtual float GetInitialMoveSpeed() => config.MoveSpeed;
        public virtual void SetMoveSpeed(float newMoveSpeed)
        {
            moveSpeed = newMoveSpeed;
            UpdateVelocity();
        }

        public virtual float GetIdleDuration() => config.IdleDuration;
        public virtual BoxCollider2D GetPlayerNearbyCollider() => config.PlayerNearbyCollider;
        public virtual BoxCollider2D GetAttackCollider() => config.AttackCollider;
        public virtual Rigidbody2D GetRigidbody() => components.Rigidbody;
        public virtual AudioClip GetAttackSound() => config.AttackSound;
        public virtual AudioClip GetDeathSound() => config.DeathSound;

        public virtual void PlayAudioClip(AudioClip clip, float volume = 1f)
        {
            if (Components.AudioSource != null || clip != null)
                Components.AudioSource.PlayOneShot(clip, volume);
        }

        public virtual AudioClip GetTakeDamageSound() => null;
        public virtual GameObject GetGemPrefab() => config.GemPrefab;
        public virtual Transform GetGemSpawnPoint() => config.GemSpawnPoint;
        public virtual int GetLives() => lives;
        public virtual float GetDirection() => direction;
        public virtual void SetDirection(float newDirection)
        {
            if (direction == newDirection) return;
            direction = newDirection;
        }

        public virtual void TurnToFacePlayer()
        {
            if (transform.right.x != -Mathf.Sign(Player.Instance.transform.position.x - transform.position.x))
                SetDirection(-direction);
        }

        public virtual void TakeDamage()
        {
            lives--;
            isTakingDamage = true;
        }

        public virtual bool IsTakingDamage() => isTakingDamage;

        public virtual void CompleteTakeDamage()
        {
            isTakingDamage = false;
            if (stateMachine.CurrentState is CommonTakeDamageState<BaseEnemy<TConfig, TComponents>> takeDamageState)
            {
                takeDamageState.OnTakeDamageComplete();
            }
        }

        public virtual void OnAttackTrigger()
        {
            if (config.AttackSound != null)
            {
                components.AudioSource.PlayOneShot(config.AttackSound);
            }

            if (IsPlayerHit(config.AttackCollider))
            {
                Player.Instance.TakeDamage();
            }
        }

        public virtual void OnAttackComplete()
        {
            if (stateMachine.CurrentState is CommonAttackState<BaseEnemy<TConfig, TComponents>> attackState)
            {
                attackState.OnAttackComplete();
            }
        }

        public virtual void OnDeathAnimationEnd()
        {
            if (lives <= 0 && Player.Instance.gameObject)
                Player.Instance.SpawnGem(config.GemPrefab, config.GemSpawnPoint.position);

            Destroy(gameObject);
        }

        public virtual void OnPlayerAttack()
        {
            stateMachine.ChangeState<CommonTakeDamageState<BaseEnemy<TConfig, TComponents>>>();
        }

        public void OnStomped() => OnPlayerAttack();

        public bool IsPlayerHit(Collider2D c)
        {
            Collider2D hit = Physics2D.OverlapBox(c.bounds.center, c.bounds.size, 0, config.PlayerLayer);

            return hit && hit.TryGetComponent<Player>(out var player);
        }

        public virtual void UpdateVelocity()
        {
            Rigidbody2D rb = GetRigidbody();
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(GetMoveSpeed() * GetDirection(), rb.linearVelocity.y);
            }
        }

        public virtual void AlignToGround()
        {
            RaycastHit2D hit = Physics2D.Raycast((Vector2)transform.position + components.Collider.offset, Vector2.down, 5f, PhysicsManager.Instance.WhatIsGround);
           // StartCoroutine(Co_GroundAlign(hit.point));
        }

        public IEnumerator Co_GroundAlign(Vector2 floorBelowPos)
        {
            yield return new WaitForSeconds(0.01f);
            transform.position = floorBelowPos + components.Collider.offset + (Vector2)components.Collider.bounds.extents;
        }

        protected IEnumerator Co_HurtFlash()
        {
            float timer = 0f;

            while (timer < .1f)
            {
                components.SpriteRenderer.color = Color.Lerp(Color.white, Color.red, timer / .1f);
                timer += Time.deltaTime;
                yield return null;
            }

            timer = 0f;
            while (timer < .1f)
            {
                components.SpriteRenderer.color = Color.Lerp(Color.red, Color.white, timer / .1f);
                timer += Time.deltaTime;
                yield return null;
            }
            components.SpriteRenderer.color = Color.white;
        }

    }

    [Serializable]
    public class EnemyComponents
    {
        public Animator Animator;
        public Collider2D Collider;
        public AudioSource AudioSource;
        public Rigidbody2D Rigidbody;
        public SpriteRenderer SpriteRenderer;
    }

    [Serializable]
    public class EnemyConfig
    {
        public float MoveSpeed = 3f;
        public float IdleDuration = 0.5f;
        public int InitialLives = 1;
        public LayerMask PlayerLayer;
        public LayerMask SpaceLayer;
        public BoxCollider2D AttackCollider;
        public BoxCollider2D PlayerNearbyCollider;
        public AudioClip AttackSound;
        public AudioClip DeathSound;
        public GameObject GemPrefab;
        public Transform GemSpawnPoint;
    }
}