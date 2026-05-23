using UnityEngine;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class BouncyMushroom : MonoBehaviour, IStompable
{

    private Animator _animator;

    private Collider2D _topCollider;
    // Top collider used for collision detection
    // Bottom collider used for disabling the top collider

    private static readonly int Idle = Animator.StringToHash("idle");
    private static readonly int Bounce = Animator.StringToHash("bounce");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _topCollider = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent<Player>(out var player))
        {
           // _topCollider.enabled = false;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent<Player>(out var player))
        {
           // _topCollider.enabled = true;
        }
    }

    public void OnBounceAnimationTrigger()
    {
        _animator.SetTrigger(Idle); // Immediately set to idle after jump
    }

    public void OnBounceAnimationEnd()
    {
        _animator.SetTrigger(Idle);
    }

    void TriggerBounce()
    {
        _animator.SetTrigger(Bounce);
        _animator.SetTrigger(Idle);
    }

    void IStompable.OnStomped() => TriggerBounce();
}