using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Splash : MonoBehaviour
{
    [SerializeField] GameObject splashPrefab;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.attachedRigidbody || collision.attachedRigidbody.linearVelocity.y >= 0)
            return;
        
        DoSplash(collision);

    }

    public void DoSplash(Collider2D collision)
    {
        GameObject instantiated = Instantiate(splashPrefab, collision.ClosestPoint(transform.position), Quaternion.identity, transform);

        if (instantiated.gameObject.TryGetComponent<ParticleSystem>(out var ps))
        {
            float splasherSpeed = -collision.attachedRigidbody.linearVelocity.y;
            var main = ps.main;
            main.startSpeed = new ParticleSystem.MinMaxCurve(0, splasherSpeed);
        }
    }
}
