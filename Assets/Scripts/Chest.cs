using UnityEngine;

public class Chest : MonoBehaviour
{
    [SerializeField] private GameObject weaponRechargeCollectiblePrefab;
    [SerializeField] private Transform collectibleSpawnPoint;
    [SerializeField] private AudioClip openSound;
    
    private static readonly int Open = Animator.StringToHash("open");

    private Animator _animator;
    private AudioSource _audioSource;

    private bool _isOpen;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent(out Player _))
        {
            GetComponent<Collider2D>().enabled = false;
            _animator.SetTrigger(Open);
            _audioSource.PlayOneShot(openSound);
        }
    }

    public void OnOpenAnimationTrigger()
    {
        Vector3 spawnPoint = collectibleSpawnPoint.position;
        spawnPoint.z = transform.position.z - 1;
        Instantiate(weaponRechargeCollectiblePrefab, spawnPoint, Quaternion.identity);
    }
}
