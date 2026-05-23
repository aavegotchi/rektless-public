using UnityEngine;

public class CollectibleHearth : MonoBehaviour
{
    [SerializeField] private AudioClip collectSound;
    
    private AudioSource _audioSource;
    
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent(out Player player))
        {
            if (!player.IsHealthFull)
            {
                GetComponent<BoxCollider2D>().enabled = false;
                player.IncreaseHealth(2);
                _audioSource.PlayOneShot(collectSound);
                GetComponent<SpriteRenderer>().enabled = false;
                Invoke(nameof(Cleanup), 1f);
            }
        }
    }

    void Cleanup()
    {
        Destroy(gameObject);
    }
}