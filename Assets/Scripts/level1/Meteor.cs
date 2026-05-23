using UnityEngine;

namespace Prefabs.level2
{
    public class Meteor : MonoBehaviour
    {
        [SerializeField] private float speed = 5f;
        [SerializeField] private int damage = 1;
        [SerializeField] private float TriggerXOffset = -5f;
        [SerializeField] private Vector3 falldirection = new Vector3(-1f, -1f, 0);
        [SerializeField] LayerMask playerLayer;
        [SerializeField] ParticleSystem WarningParticles;

        private static readonly int Explode = Animator.StringToHash("explode");

        private bool hasStartedFalling;
        private bool stop;

        private void OnEnable()
        {
            transform.Translate(Vector2.right * UnityEngine.Random.Range(-10f, 10f));
            TriggerXOffset += UnityEngine.Random.Range(-5f, 0);
        }

        private void Update()
        {
            if (!hasStartedFalling)
            {
                CheckForPlayer();
                return;
            }

            if (stop) return;

            transform.Translate(falldirection * (speed * Time.deltaTime));
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Ground"))
            {
                Collider2D mCollider = GetComponent<Collider2D>();
                mCollider.enabled = false;
                stop = true;
                speed = 0;

                GetComponent<Animator>().SetTrigger(Explode);
            }

            if (other.CompareTag("Player"))
            {
                Player.Instance.TakeDamage(damage);
            }
        }

        private void CheckForPlayer()
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position + (TriggerXOffset * Vector3.right), falldirection, 200f, PhysicsManager.Instance.WhatIsPlayer);
            if (hit && hit.collider.TryGetComponent<Player>(out var _))
            {
                WarningParticles.Play();
                hasStartedFalling = true;
            }
        }

        public void OnExplodeAnimationEnd()
        {
            gameObject.SetActive(false);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position + (TriggerXOffset * Vector3.right), falldirection);
        }


    }
}