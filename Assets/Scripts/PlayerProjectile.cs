using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    [SerializeField] private LayerMask enemyLayer;

    private Collider2D _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        if (!_collider.enabled)
        {
            return;
        }

        if (IsOutOfScreen())
        {
            _collider.enabled = false;
            Destroy(gameObject, 0.5f);
            return;
        }

        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, Vector2.one * 0.1f, 0, enemyLayer);
        bool hasHit = false;
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out IAttackable enemy))
            {
                hasHit = true;
                enemy.OnPlayerAttack();
            }
        }

        if (hasHit)
        {
            _collider.enabled = false;
            Destroy(gameObject, 0.5f);
        }
    }

    private bool IsOutOfScreen()
    {
        var viewportPosition = Camera.main.WorldToViewportPoint(transform.position);
        return viewportPosition.x < 0 || viewportPosition.x > 1 || viewportPosition.y < 0 || viewportPosition.y > 1;
    }
}