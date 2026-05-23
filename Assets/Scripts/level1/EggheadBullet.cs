using UnityEngine;
using System.Collections;

namespace level2
{
    public class EggheadBullet : MonoBehaviour, IAttackable
    {
        [SerializeField] private int damage = 1;
        [SerializeField] private float timeout = 10f;
        [SerializeField] private float launchForceX = 5f;
        [SerializeField] private float launchForceY = 10f;
        [SerializeField] private AudioClip explodeSound;

        private Animator _animator;
        private Rigidbody2D _rigidbody;
        private AudioSource _audioSource;
            
        private static readonly int Explode = Animator.StringToHash("explode");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            StartCoroutine(TimeoutDestroy());
            Launch();
        }

        private void Launch()
        {
            Vector2 launchForce = new Vector2(DetermineXDirection() * launchForceX, launchForceY);
            _rigidbody.AddForce(launchForce, ForceMode2D.Impulse);
        }

        private int DetermineXDirection()
        {
            if (Player.Instance != null)
            {
                // Return 1 if player is to the right, -1 if to the left
                return (Player.Instance.transform.position.x > transform.position.x) ? 1 : -1;
            }

            // Default to 1 if player instance is not found
            return 1;
        }

        private IEnumerator TimeoutDestroy()
        {
            yield return new WaitForSeconds(timeout);
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("idle")) return;

            if (other.CompareTag("Player"))
            {
                _audioSource.PlayOneShot(explodeSound);
                OnPlayerAttack();
                return;
            }

            if (other.CompareTag("Ground") && _rigidbody.linearVelocity.y < 0)
            {
                _audioSource.PlayOneShot(explodeSound);
                GetComponent<BoxCollider2D>().enabled = false;
                _rigidbody.linearVelocity = Vector2.zero;
                _rigidbody.isKinematic = true;
                _animator.SetTrigger(Explode);
            }
        }

        public void ExplodeComplete()
        {
            Destroy(gameObject);
        }

        public void OnPlayerAttack()
        {
            GetComponent<BoxCollider2D>().enabled = false;
            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.isKinematic = true;
            Player.Instance.TakeDamage(damage);
            _animator.SetTrigger(Explode);
            _audioSource.PlayOneShot(explodeSound);
        }
    }
}