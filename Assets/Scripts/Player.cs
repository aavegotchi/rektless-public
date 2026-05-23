using System;
using System.Collections;
using System.Collections.Generic;
using Health;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Weapons.Base;

public class Player : MonoBehaviourSingleton<Player>
{
    [Header("Movement Settings")] [SerializeField]
    private float initialMoveSpeed = 5f;

    [SerializeField] private float initialCrouchMoveSpeed = 3f;
    [SerializeField] private Vector2 crouchWalkColliderOffset = new Vector2(0, -0.5f);
    [SerializeField] private Vector2 crouchWalkColliderSize = new Vector2(1, 1);
    Vector2 mainColliderOffset = new();
    Vector2 mainColliderSize = new();
    [SerializeField] private float godTime = 2f;
    [SerializeField] private int initialLives = 20;
    [SerializeField] private int initialAxes = 3;
    [SerializeField] private GameObject axePrefab;
    private RuntimeAnimatorController projectileAnimator;
    [SerializeField] private LayerMask enemyLayer; // bad naming but this not enemy layer exactly. It also includes ground because of falling platform
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private BoxCollider2D attackCollider;
    private float startPosX = 0f;

    [Header("AudioSettings")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip doubleJumpSound;
    [SerializeField] private AudioClip fallDeathSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip punchSound;
    [SerializeField] private AudioClip punchWithoutHitSound;

    [Header("Jump Settings")]
    [SerializeField] private float maxJumpHeight = 4f;
    [SerializeField] private float timeToJumpApex = 0.4f;
    [SerializeField] private float accelerationTimeAirborne = 0.2f;
    [SerializeField] private float accelerationTimeGrounded = 0.1f;
    [SerializeField] private float jumpBufferTime = .25f;
    [SerializeField] private float coyoteTime = .2f;

    private bool isJumping;
    private float jumpStartTime;
    private float jumpVelocity;
    private float gravity;
    private Vector3 velocity;
    private float velocityXSmoothing;

    private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] CapsuleCollider2D mainCollider;
    [SerializeField] HealthBars healthBars;
    [SerializeField] GameObject healthBarPrefab;
    private AudioSource audioSource;
    [SerializeField] SpriteRenderer spriteRenderer;

    private int lives;
    public int CurrentWeapons { get; private set; }
    public int MaxWeapons = 15;
    private float moveSpeed;
    private float currentGodTime;
    private float moveDirection;

    private bool isGrounded;
    private float lastGroundedTime;
    private float lastBounceTime;

    private bool isFrozen;
    private bool isAttacking;

    private static readonly int Hit = Animator.StringToHash("hit");
    private static readonly int Death = Animator.StringToHash("death");
    private static readonly int Attack = Animator.StringToHash("attack");
    private static readonly int RangeAttack = Animator.StringToHash("range_attack");
    private static readonly int Jump1 = Animator.StringToHash("jump");
    private static readonly int Crouch = Animator.StringToHash("crouch");
    private static readonly int VelocityYDirection = Animator.StringToHash("velocity_y_direction");
    private static readonly int Velocity = Animator.StringToHash("velocity");
    private static readonly int IsSliding = Animator.StringToHash("is_sliding");

    public float MoveDirection => moveDirection;
    public CapsuleCollider2D MainCollider => mainCollider;
    public Rigidbody2D Rb => rb;
    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public bool BossActive { get; set; } = false;
    public bool OnStarting { get; set; } = true;
    public Action OnStartAction { get; set; }

    private bool disableControlsAndColliders;

    public bool DisableControlsAndColliders
    {
        get => disableControlsAndColliders;
        set
        {
            disableControlsAndColliders = value;
            rb.linearVelocity = Vector2.zero;
            rb.simulated = !value;
            mainCollider.enabled = !value;
            attackCollider.enabled = !value;
        }
    }

    private Vector2 lastPosition;
    private Vector2 movementDelta;

    public bool IsHealthFull => lives == initialLives;
    public Vector2 LastPosition => lastPosition;
    public Vector2 MovementDelta => movementDelta;
    public float DistanceStatistic { get; private set; }
    public int KillsStatistic { get; set; }
    public int GemsStatistic { get; set; }

    [Header("Move Settings")]
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float slideThreshold = 1f;
    [SerializeField] private float turnSpeed = 10f;

    private float currentSpeed = 0f;

    [Header("Crouch Slide")]
    [SerializeField] private float crouchSlideDeceleration = 15f;
    [SerializeField] private float crouchSlideDuration = 0.5f;
    [SerializeField] private float crouchSlideThreshold = 1f;
    [SerializeField] private float crouchSlideDistance = 1.3f;

    private bool isCrouchSliding = false;
    private float crouchSlideTimer = 0f;

    [Header("Dash Settings")]
    [SerializeField]
    private float dashDuration = .5f, dashSpeed = 8f, dashTimer = 0f;
    [SerializeField]
    private bool isDashing;
    [SerializeField] float echoInterval = .05f;
    [SerializeField] EchoPool echoPool;
    float lastEchoTime;

    [Header("Environment Settings")] 
    [SerializeField]
    private float fallDeathYThreshold = -6.15f;
    [SerializeField] GameObject fallDeathGhostPrefab;

    [Header("Hurt Settings")]
    [SerializeField] Vector2 screenshakeIntensity = new();
    [SerializeField] float screenshakeDuration = .3f;
    

    public void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        mainCollider = GetComponent<CapsuleCollider2D>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        healthBars.InitializeHealthBars(initialLives, healthBarPrefab);
        mainColliderSize = mainCollider.size;
        mainColliderOffset = mainCollider.offset;

        echoPool = (EchoPool)GameObject.Instantiate(echoPool);

        lives = initialLives;
        CurrentWeapons = initialAxes;
        moveSpeed = initialMoveSpeed;
        lastPosition = transform.position;
        startPosX = transform.position.x;

        BossSpawnManager.OnBossSpawned += OnBossSpawnedF;
        BossSpawnManager.OnReadyToSpawn += OnReadyToSpawnF;

        RestartManager.Instance.BeforeRestart += OnDisable;

        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        rb.gravityScale = 0f; // We'll handle gravity manually

        StartCoroutine(ChangeTexture());
    }

    private void Update()
    {
        if (PlayerIsBelowFallThreshold())
        {
            EnvironmentKill();
            return;
        }

        if (PlayerIsBehindWall())
        {
            DieFromFatalDamage();
            return;
        }

        UpdateDistanceStatistic();

        UpdateGroundedState();
        HandleLanding();
        HandleJump();
        
        ApplyGravity();
        UpdateHorizontalMovement();
        MovePlayer();
        HandleEcho();

        if (disableControlsAndColliders) return;
        HandleInput();
        HandleDash();
        UpdateGodMode();
        ApplyMomentum();
        CheckStompCollider();
        CheckHeadCollision();
    }

    IEnumerator ChangeTexture()
    {
        var charTex = PersistentData.Instance.CurrentCharacter.inGameTexture;
        if (charTex != null)
        {
            spriteRenderer.material.SetTexture("_SwapTex", charTex);
        }
        else
        {
            Debug.LogWarning("Using default texture");
        }

        yield return null;
    }

    private void OnDisable()
    {
        BossSpawnManager.OnBossSpawned -= OnBossSpawnedF;
        BossSpawnManager.OnReadyToSpawn -= OnReadyToSpawnF;

        RestartManager.Instance.BeforeRestart -= OnDisable;
    }

    public void IncreaseHealth(int amount)
    {
        if (lives + amount > initialLives)
        {
            amount = initialLives - lives;
        }

        lives += amount;
        healthBars.IncreaseHealthBar(amount);
    }

    public void AddWeapons(int amount)
    {
        CurrentWeapons += amount;
        if (amount > MaxWeapons)
            amount = MaxWeapons;
    }

    private void OnBossSpawnedF()
    {
        BossActive = true;
    }

    public void OnBossStartToDeathAnim()
    {
        isFrozen = true;
        moveDirection = 0f;
        rb.linearVelocity = Vector2.zero;
        animator.SetBool(IsSliding, false);
    }

    private void OnReadyToSpawnF()
    {
        BossActive = false;
        isFrozen = false;
    }


    private void UpdateGroundedState()
    {
        Vector2 playerCenterBottom = new Vector2(mainCollider.bounds.center.x, mainCollider.bounds.min.y);

        // Player center
        Collider2D hit = Physics2D.OverlapBox(playerCenterBottom, new Vector2(mainCollider.bounds.size.x - .1f, .1f),
            0f, groundLayer);


        if(hit)
        {
            isGrounded = true;
            lastGroundedTime = Time.time;
            return;
        }
        isGrounded = false;
    }

    private void HandleLanding()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = 0;
            animator.SetBool(Jump1, false);
        }
    }

    private void HandleJump()
    {
        if (ShouldApplyFasterFall())
            ApplyFasterFall();
    }

    private bool ShouldApplyFasterFall()
    {
        return (!isDashing && Time.time - lastBounceTime > .4f && !InputManager.Instance.jumpPressed && IsPlayerAscending());
    }

    private bool IsPlayerAscending()
    {
        return velocity.y > 0;
    }

    private void ApplyFasterFall()
    {
        isJumping = false;
        velocity.y += gravity * Time.deltaTime * 2; // Fall faster when jump button is released
    }

    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
    }

    private void UpdateHorizontalMovement()
    {
        float currentMoveSpeed = isDashing ? dashSpeed : moveSpeed;
        float targetVelocityX = moveDirection * currentMoveSpeed;

        float smoothTime = isGrounded ? accelerationTimeGrounded : accelerationTimeAirborne;

        if (isDashing)
        {
            velocity.x = targetVelocityX;
            return;
        }

        
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, smoothTime);
    }

    private void MovePlayer()
    {
        rb.linearVelocity = velocity;
    }

    private void FixedUpdate()
    {
        if (disableControlsAndColliders) return;
        Move();
    }

    private void HandleInput()
    {
        if (isFrozen) return;

        moveDirection = !isFrozen ? InputManager.Instance.MoveInputInt.x : moveDirection;

        if (Time.time - InputManager.Instance.lastJumpPressTime <= jumpBufferTime
            && (isGrounded || Time.time - lastGroundedTime <= coyoteTime))
        {
            Jump(jumpVelocity);
        }

        if (!isAttacking && InputManager.Instance.meleePressedThisFrame)
        {
            animator.SetBool(Attack, true);
            isAttacking = true;
        }

        if (!isAttacking && InputManager.Instance.firePressedThisFrame && CurrentWeapons > 0)
        {
            ThrowAxe();
        }

        if (!isAttacking && InputManager.Instance.dashPressedThisFrame)
        {
            StartDash();
        }

        if (isGrounded && InputManager.Instance.MoveInputInt.y == -1)
        {
            if (Mathf.Abs(currentSpeed) > crouchSlideThreshold)
            {
                StartCrouchSlide();
            }
            else
            {
                StartCrouch();
            }
        }
        else if (InputManager.Instance.MoveInputInt.y != -1 ||
                 (InputManager.Instance.MoveInputInt.y != -1 && (animator.GetBool(Crouch) || isCrouchSliding)))
        {
            EndCrouch();
        }
    }

    private void StartCrouch()
    {
        animator.SetBool(Crouch, true);
        mainCollider.offset = crouchWalkColliderOffset;
        mainCollider.size = crouchWalkColliderSize;
        moveSpeed = initialCrouchMoveSpeed;
        currentSpeed = 0f; // Stop movement when crouching
    }

    private void StartCrouchSlide()
    {
        isCrouchSliding = true;
        crouchSlideTimer = crouchSlideDuration * crouchSlideDistance;
        animator.SetBool(Crouch, true);
        animator.SetBool(IsSliding, true);

        mainCollider.offset = crouchWalkColliderOffset;
        mainCollider.size = crouchWalkColliderSize;
    }

    private void EndCrouch()
    {
        animator.SetBool(Crouch, false);

        mainCollider.offset = mainColliderOffset;
        mainCollider.size = mainColliderSize;


        moveSpeed = initialMoveSpeed;
        isCrouchSliding = false;
        crouchSlideTimer = 0f;
    }

    private void StartDash()
    {
        if (isDashing)
            return;

        animator.SetBool(IsSliding, true);
        isDashing = true;
        dashTimer = dashDuration;
        mainCollider.offset = crouchWalkColliderOffset;
        mainCollider.size = crouchWalkColliderSize;

    }

    private void EndDash()
    {
        animator.SetBool(IsSliding, false);
        isDashing = false;
        mainCollider.offset = mainColliderOffset;
        mainCollider.size = mainColliderSize;
    }

    private void HandleDash()
    {
        if (!isDashing) return;

        animator.SetBool(IsSliding, true);
        dashTimer -= Time.deltaTime;
        mainCollider.offset = crouchWalkColliderOffset;
        mainCollider.size = crouchWalkColliderSize;

        if (!isGrounded) return;

        if (dashTimer < 0f || !InputManager.Instance.dashPressed)
            EndDash();
    }

    private void HandleEcho()
    {
        if (isDashing && Time.time > lastEchoTime + echoInterval)
        {
            echoPool.AddEcho(transform, spriteRenderer.sprite);
            lastEchoTime = Time.time;
        }
    }

    private void Move()
    {
        if (isFrozen) return;

        animator.SetFloat(VelocityYDirection, isGrounded ? 0 : velocity.y);
        animator.SetFloat(Velocity, Mathf.Abs(currentSpeed));
        if (!isFrozen)
        {
            if (isCrouchSliding)
            {
                ApplyCrouchSlide();
            }
            else
            {
                UpdatePlayerOrientation();
            }
        }

        movementDelta = (Vector2)transform.position - lastPosition;
        lastPosition = transform.position;
    }

    private void UpdatePlayerOrientation()
    {
        if (Mathf.Abs(currentSpeed) > 0.1f)
        {
            bool isMovingRight = currentSpeed > 0;
            bool wantsToMoveRight = moveDirection > 0;

            if (isMovingRight != wantsToMoveRight && Mathf.Abs(currentSpeed) > slideThreshold)
            {
                EndDash();
                // Player is sliding
                animator.SetBool(IsSliding, true);

                // Smoother rotation when changing direction
                float targetRotation = wantsToMoveRight ? 0 : 180;
                float currentRotation = transform.eulerAngles.y;
                float newRotation = Mathf.MoveTowardsAngle(currentRotation, targetRotation, turnSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0, newRotation, 0);
            }
            else
            {
                animator.SetBool(IsSliding, false);
                transform.rotation = Quaternion.Euler(0, isMovingRight ? 0 : 180, 0);
            }
        }
        else
        {
            animator.SetBool(IsSliding, false);
        }
    }

    public void Bounce(float bounceVelocity, bool canJumpFrom)
    {
        audioSource.PlayOneShot(doubleJumpSound);
        lastBounceTime = Time.time;
        if (canJumpFrom && InputManager.Instance.jumpPressed)
            bounceVelocity *= 1.5f;

        Jump(bounceVelocity, true);
    }

    public void Jump(float _jumpVelocity, bool forceJump = false)
    {
        if (velocity.y > 0)
            return;

        isJumping = true;
        jumpStartTime = Time.time;
        velocity.y = _jumpVelocity;
        animator.SetBool(Jump1, true);
        animator.SetBool(Attack, false);
        isAttacking = false;
        audioSource.PlayOneShot(jumpSound);
    }

    public void Anim_Attack()
    {
        StartCoroutine(Co_Attack());
    }
    private IEnumerator Co_Attack()
    {
        isAttacking = true;

        var attackDuration = .2f;

        List<IAttackable> attacked = new();
        Collider2D[] enemies = new Collider2D[5];
        while (attackDuration >0)
        {
            enemies = Physics2D.OverlapBoxAll(attackCollider.bounds.center, attackCollider.bounds.size, 0, enemyLayer);

            foreach (var enemy in enemies)
            {
                if (enemy != null && enemy.TryGetComponent<IAttackable>(out var attackable) && !attacked.Contains(attackable))
                {
                    attackable.OnPlayerAttack();
                    attacked.Add(attackable);
                }
            }
            attackDuration -= Time.deltaTime;
            yield return null;
        }

        audioSource.PlayOneShot(enemies.Length > 0 ? punchSound : punchWithoutHitSound);
        isAttacking = false;
    }

    private void ThrowAxe()
    {
        isAttacking = true;
        animator.SetBool(RangeAttack, true);
        CurrentWeapons--;
        Vector3 spawnPosition = new Vector3(transform.position.x + (moveDirection * mainCollider.size.x),
            transform.position.y, 0);

        GameObject axeObject = Instantiate(axePrefab, spawnPosition, Quaternion.identity);
        var thrownAxe = axeObject.GetComponent<ThrowableWeapon>();
        Vector2 throwDirection = new Vector2(transform.rotation.eulerAngles.y == 180 ? -1 : 1, 0);
        thrownAxe.Use(throwDirection);

        if (projectileAnimator == null)
        {
            projectileAnimator = Data.Instance.DefaultProjectileAnimator;
           // projectileAnimator = Data.Instance.GetProjectileAnimatorComponentByCharacterName(PersistentData.Instance.CurrentCharacter.name);
            //projectileAnimator = projectileAnimator ? projectileAnimator : Data.Instance.DefaultProjectileAnimator;
        }
        axeObject.GetComponent<Animator>().runtimeAnimatorController = projectileAnimator;
    }

    private void UpdateGodMode()
    {
        if (currentGodTime > 0)
        {
            currentGodTime -= Time.deltaTime;
            if (currentGodTime <= 0)
            {
                currentGodTime = 0;
                animator.SetBool(Hit, false);
            }
        }
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("TouchEnemy"))
        {
            TakeDamage();
        }
    }

    public void TakeDamage(int _lives = 1, bool death = false)
    {
        if (PersistentData.Instance != null && PersistentData.Instance.DebugInfiniteLife)
            return;
        
        if (death)
        {
            CameraFollow.Instance.Screenshake(screenshakeDuration, screenshakeIntensity);
            gameObject.SetActive(false);
            GameUiManager.Instance.ShowGameOverPanel();
            PlayfabManager.Instance.SendLeaderboard(Mathf.FloorToInt(DistanceStatistic), KillsStatistic, GemsStatistic, PersistentData.Instance.CurrentLevelConfig.LeaderboardKey);
            return;
        }

        if (currentGodTime > 0) return;

        CameraFollow.Instance.Screenshake(screenshakeDuration, screenshakeIntensity);
        audioSource.PlayOneShot(hitSound);
        lives -= _lives;
        healthBars.DecreaseHealthBar(_lives);
        currentGodTime = godTime;
        animator.SetBool(Hit, true);
        animator.SetBool(Attack, false);
        isAttacking = false;

        if (lives <= 0)
        {
            isFrozen = true;
            animator.SetTrigger(Death);
            disableControlsAndColliders = true;
            PlayfabManager.Instance.SendLeaderboard(Mathf.FloorToInt(DistanceStatistic), KillsStatistic, GemsStatistic, PersistentData.Instance.CurrentLevelConfig.LeaderboardKey);
            Invoke(nameof(OnDeathAnimationEnd), 1.5f);
        }
    }

    public void Push(float force, Vector2 direction)
    {
        StartCoroutine(HandlePush(force, direction.normalized));
    }

    private IEnumerator HandlePush(float force, Vector2 direction)
    {
        isFrozen = true;
        moveDirection = 0f;
        animator.SetBool(IsSliding, false);
        rb.linearVelocity = Vector2.zero;
        float timer = .5f;
        rb.AddForce(direction * force, ForceMode2D.Impulse);
        while (timer > 0f)
        {  
            rb.AddForce(direction * force * .2f, ForceMode2D.Impulse);
            ApplyFasterFall();
            timer -= Time.deltaTime;
            yield return null;
        }

        isFrozen = false;
    }

    public void OnAttackAnimationRealEnd()
    {
        isAttacking = false;
        animator.SetBool(Attack, false);
    }

    public void OnRangeAttackAnimationEnd()
    {
        isAttacking = false;
        animator.SetBool(RangeAttack, false);
    }

    public void OnDeathAnimationEnd()
    {
        gameObject.SetActive(false);
        GameUiManager.Instance.ShowGameOverPanel();
    }

    public void OnHitAnimationEnd()
    {
        animator.SetBool(Hit, false);
    }

    private const float GEM_SEPARATOR = 0.9f;

    public void SpawnGem(GameObject gem, Vector3 position)
    {
        int random = UnityEngine.Random.Range(0, 100);
        if (random < 50)
        {
            return;
        }

        if (random < 85)
        {
            Instantiate(gem, position, Quaternion.identity);
        }
        else
        {
            Instantiate(gem, position + new Vector3(GEM_SEPARATOR / 2, 0, 0), Quaternion.identity);
            Instantiate(gem, position - new Vector3(GEM_SEPARATOR / 2, 0, 0), Quaternion.identity);
        }
    }

    private void ApplyMomentum()
    {
        if (isCrouchSliding) return; // Don't apply regular momentum while crouch sliding
        if (isDashing) return;

        float targetSpeed = moveDirection * maxSpeed;

        if (moveDirection != 0)
        {
            // Faster acceleration when changing direction
            float currentAcceleration =
                (Mathf.Sign(currentSpeed) != Mathf.Sign(targetSpeed)) ? acceleration * 2f : acceleration;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, currentAcceleration * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.deltaTime);
        }

        velocity.x = currentSpeed;
    }

    private void ApplyCrouchSlide()
    {
        crouchSlideTimer -= Time.deltaTime;
        if (crouchSlideTimer <= 0)
        {
            EndCrouch();
            return;
        }

        // Adjust deceleration based on remaining slide time
        float adjustedDeceleration = crouchSlideDeceleration *
                                     (1 - (crouchSlideTimer / (crouchSlideDuration * crouchSlideDistance)));
        currentSpeed = Mathf.MoveTowards(currentSpeed, 0, adjustedDeceleration * Time.deltaTime);
        velocity.x = currentSpeed;
    }

    private bool PlayerIsBelowFallThreshold()
    {
        return transform.position.y <= fallDeathYThreshold;
    }

    public void EnvironmentKill()
    {
        PlayFallDeathSound();
        SpawnAngel();
        DieFromFatalDamage();
    }

    private bool PlayerIsBehindWall()
    {
        return Wall.Instance.HasLeftOfWall(mainCollider);
    }

    private void DieFromFatalDamage()
    {
        TakeDamage(lives, true);
    }

    private void PlayFallDeathSound()
    {
        GameUiManager.Instance.GetComponent<AudioSource>().PlayOneShot(fallDeathSound);
    }

    private void SpawnAngel()
    {
        Instantiate(fallDeathGhostPrefab, transform.position, Quaternion.identity);
    }

    private void UpdateDistanceStatistic()
    {
        float newDistance = Mathf.Abs(transform.position.x - startPosX);
        DistanceStatistic = newDistance > DistanceStatistic ? newDistance : DistanceStatistic;
    }

    private void CheckStompCollider()
    {
        if (velocity.y >= 0) return;

        Vector2 playerCenterBottom = new Vector2(mainCollider.bounds.center.x, mainCollider.bounds.min.y + .05f);

        // Player center
        var hits = Physics2D.OverlapBoxAll(playerCenterBottom, new Vector2(mainCollider.bounds.size.x, .2f),
            0f, enemyLayer);
        foreach (var hit in hits)
        {
            Vector2 enemyCenterTop = new Vector2(hit.bounds.center.x, hit.bounds.max.y);
            if (hit is CapsuleCollider2D capsule)
            {
                switch (capsule.direction)
                {
                    case CapsuleDirection2D.Vertical:
                        enemyCenterTop.y -= capsule.bounds.extents.x;
                        break;
                    case CapsuleDirection2D.Horizontal:
                        enemyCenterTop.y -= capsule.bounds.extents.y;
                        break;
                };
            }

            if (playerCenterBottom.y + 0.2 < enemyCenterTop.y)
            {
                return;
            }
            
            if (hit.gameObject.TryGetComponent(out IStompable stompable))
            {
                stompable.OnStomped();

                if (hit.gameObject.TryGetComponent(out IBounceable bounceable))
                {
                    Bounce(bounceable.BounceSpeed, bounceable.CanJumpToAddMoreHeight);
                }

                return;
            }
        }
    }

    private void CheckHeadCollision()
    {
        RaycastHit2D hit = Physics2D.Raycast(mainCollider.bounds.center, Vector2.up,
            mainCollider.bounds.extents.y + 0.1f, groundLayer);
        if (hit.collider != null)
        {
            velocity.y = -jumpVelocity * 0.15f;
        }
    }

}
