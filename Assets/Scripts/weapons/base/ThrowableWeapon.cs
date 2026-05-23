using UnityEngine;

namespace Weapons.Base
{
    public abstract class ThrowableWeapon : MonoBehaviour
    {
        [SerializeField] protected float throwSpeed = 13f;
        [SerializeField] protected float throwAngle = 20f;
        [SerializeField] protected float gravityScale = 1f;

        protected Rigidbody2D rb;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void Use(Vector2 direction)
        {
            Throw(direction);
        }

        protected virtual void Throw(Vector2 direction)
        {
            float angleInRadians = throwAngle * Mathf.Deg2Rad;
            Vector2 throwVelocity = new Vector2(
                Mathf.Cos(angleInRadians) * throwSpeed * Mathf.Sign(direction.x),
                Mathf.Sin(angleInRadians) * throwSpeed
            );
            rb.linearVelocity = throwVelocity;
            rb.gravityScale = gravityScale;
        }
    }
}