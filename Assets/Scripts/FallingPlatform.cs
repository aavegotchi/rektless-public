using UnityEngine;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class FallingPlatform : MonoBehaviour, IStompable
{
    private Animator _animator;

    private static readonly int Play = Animator.StringToHash("play");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void OnStomped()
    {
        _animator.SetTrigger(Play);
    }

    public void OnPlayAnimationTrigger()
    {
        GetComponent<Collider2D>().enabled = false;
    }

    public void OnPlayAnimationEnd()
    {
        Destroy(gameObject);
    }
}