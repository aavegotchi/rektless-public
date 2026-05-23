using UnityEngine;
using System.Collections;
using System;

public class Wall : MonoBehaviourSingleton<Wall>
{
    [SerializeField] private float _beginningMoveSpeed = 1.5f;
    [SerializeField] private float _beginningMoveSpeedDuration = 2f;
    [SerializeField] private float _normalMoveSpeed = 2f;
    [SerializeField] private float _maxMoveSpeed = 10f;
    [SerializeField] private float _maxSpeedAtDistance = 20f;
    [SerializeField] private float _slowMoveSpeed = 1f; // Speed when slowed down
    [SerializeField] private float _slowdownDuration = 2f; // Duration of slowdown in seconds
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private BoxCollider2D _attackCollider;
    [SerializeField] private AudioClip _attackSound;

    private BoxCollider2D _collider;

    private Animator _animator;
    private AudioSource _audioSource;

    [SerializeField] float _currentMoveSpeed;
    private bool _isSlowedDown;
    private bool _beginningFinished;

    private static readonly int Attack = Animator.StringToHash("attack");
    private static readonly int Idle = Animator.StringToHash("idle");

    private Func<float> GetSpeed;

    public void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _currentMoveSpeed = _beginningMoveSpeed;
        GetSpeed = RegularSpeed;
    }

    private void Start()
    {
        StartCoroutine(StartMoveSpeedCoroutine());
    }

    private IEnumerator StartMoveSpeedCoroutine()
    {
        yield return new WaitForSeconds(_beginningMoveSpeedDuration);
        _beginningFinished = true;
        GetSpeed = RegularSpeed;
    }

    private void Update()
    {
        if (!Player.Instance.gameObject.activeInHierarchy) return;
        if (Player.Instance.BossActive) return;
        if (Player.Instance.OnStarting) return;

        _currentMoveSpeed = GetSpeed.Invoke();
        transform.Translate(Vector2.right * (GetSpeed() * Time.deltaTime));
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (Player.Instance.BossActive) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            if (!_isSlowedDown)
            {
                StartCoroutine(SlowdownCoroutine());
            }
        }
    }

    public void StartAttack()
    {
        _animator.SetTrigger(Attack);
    }

    private IEnumerator SlowdownCoroutine()
    {
        _isSlowedDown = true;
        GetSpeed = SlowedSpeed;
        yield return new WaitForSeconds(_slowdownDuration);
        GetSpeed = RegularSpeed;
        _isSlowedDown = false;
    }

    public bool HasLeftOfWall(Collider2D c)
    {
        return c.bounds.max.x < _collider.bounds.min.x;
    }

    public void OnAttackTrigger()
    {
        _audioSource.PlayOneShot(_attackSound);
        Collider2D hit =
            Physics2D.OverlapBox(_attackCollider.bounds.center, _attackCollider.bounds.size, 0, _playerLayer);
        if (hit)
        {
            Player.Instance.TakeDamage();
        }
    }

    public void OnAttackEnd()
    {
        _animator.SetTrigger(Idle);
    }

    private float SlowedSpeed()
    {
        return _beginningFinished? _slowMoveSpeed : _beginningMoveSpeed;
    }

    private float RegularSpeed()
    {
        return _beginningFinished ? Mathf.Lerp(_normalMoveSpeed, _maxMoveSpeed, (float)Player.Instance.DistanceStatistic / _maxSpeedAtDistance) : _beginningMoveSpeed;
    }
}