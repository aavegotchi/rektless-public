using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Events;

public class SimpleHazard : MonoBehaviour
{

    [SerializeField] LayerMask PlayerLayer;
    [SerializeField] int damageAmount = 1;
    [SerializeField]
    bool lethal;

    public UnityEvent<Collider2D> OnImpact;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnImpact?.Invoke(collision);
        if (collision.TryGetComponent<Player>(out var player))
        {
            player.TakeDamage(damageAmount);
            if (lethal)
                player.EnvironmentKill();
        }
    }

}
