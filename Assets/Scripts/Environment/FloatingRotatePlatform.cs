using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingRotatePlatform : MonoBehaviour
{
    public Vector2 contactPoint = Vector3.zero;
    public Vector3 angularVelocity = Vector3.zero;
    public Vector3 lastAngularVelocity = Vector3.zero;
    [SerializeField] float springConstant =.99f;
    [SerializeField] float drag = .99f;
    [SerializeField] float reactivenessToWeight;
    Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        contactPoint = transform.position;
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if ( collision.collider.TryGetComponent<Player>(out var player))
        {
            contactPoint = player.transform.position;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent<Player>(out var player))
            contactPoint = transform.position;
    }


    private void FixedUpdate()
    {
        rb.AddTorque(rb.rotation * -springConstant);
    }
    

}
