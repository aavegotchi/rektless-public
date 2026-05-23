using UnityEngine;
using System.Collections;

namespace level2
{
    public class RocketBullet : MonoBehaviour, IStompable
    {
        [SerializeField] private float speed = 5f;
        [SerializeField] private int damage = 1;
        [SerializeField] private float timeout = 10f;
        [SerializeField] private AudioClip explodeSound;

        private Animator _animator;
        private AudioSource _audioSource;
        
        private float _timeElapsed;
        private float _direction = 1;

        private static readonly int Explode = Animator.StringToHash("explode");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _audioSource = GetComponent<AudioSource>();

            _direction = -transform.right.x;
            transform.rotation = Quaternion.Euler(0, _direction == -1 ? 0 : 180, 0);
        }

        private void Update()
        {
            _timeElapsed += Time.deltaTime;
            if (_timeElapsed >= timeout)
            {
                Destroy(gameObject);
            }

            Vector3 newPosition = transform.position;
            newPosition.x += _direction * speed * Time.deltaTime;
            transform.position = newPosition;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (other.transform.position.y > transform.position.y + 0.5f) // Check if player is above the bullet
                {
                    return;
                }
                else
                {
                    GetComponent<BoxCollider2D>().enabled = false;
                    _direction = 0;
                    Player.Instance.TakeDamage(damage);
                    _animator.SetTrigger(Explode);
                    _audioSource.PlayOneShot(explodeSound);
                }
            }
        }

        public void ExplodeComplete()
        {
            Destroy(gameObject);
        }

        public void OnStomped()
        {
            GetComponent<BoxCollider2D>().enabled = false;
            _direction = 0;
            Player.Instance.KillsStatistic++;
            _animator.SetTrigger(Explode);
            _audioSource.PlayOneShot(explodeSound);
            StartCoroutine(DelayedDestruction());
        }

        private IEnumerator DelayedDestruction()
        {
            yield return new WaitForSeconds(0.1f); // Adjust this time as needed
            GetComponent<BoxCollider2D>().enabled = false;
            yield return new WaitForSeconds(0.4f); // Adjust this time as needed
            Destroy(gameObject);
        }
    }
}