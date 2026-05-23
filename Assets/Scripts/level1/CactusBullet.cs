using UnityEngine;

namespace level2
{
    public class CactusBullet : MonoBehaviour, IAttackable
    {
        [SerializeField] private float speed = 5f;
        [SerializeField] private int damage = 1;
        [SerializeField] private float timeout = 10f;

        private Animator _animator;
        private float _timeElapsed;
        private float _direction = 1;

        private static readonly int Explode = Animator.StringToHash("explode");

        private void Awake()
        {
            _direction = -transform.right.x;
        }

        private void Start()
        {
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            _timeElapsed += Time.deltaTime;
            if (_timeElapsed >= timeout)
            {
                _animator.SetTrigger(Explode);
            }

            Vector3 newPosition = transform.position;
            newPosition.x += _direction * speed * Time.deltaTime;
            transform.position = newPosition;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                GetComponent<BoxCollider2D>().enabled = false;
                _direction = 0;
                Player.Instance.TakeDamage(damage);
                _animator.SetTrigger(Explode);
            }
        }

        public void OnPlayerAttack()
        {
            GetComponent<BoxCollider2D>().enabled = false;
            _direction = 0;
            _animator.SetTrigger(Explode);
        }

        public void ExplodeComplete()
        {
            gameObject.SetActive(false);
        }
    }
}