using UnityEngine;

public class CollectibleGem : MonoBehaviour
{
    [SerializeField]
    AudioClip collectSound;
    [SerializeField]
    AudioSource audioSource;

    private void OnEnable()
    {
        var animator = GetComponent<Animator>();
        animator.runtimeAnimatorController = PersistentData.Instance.CurrentLevelConfig.GemAnimator;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player.Instance.GemsStatistic++;
            audioSource.pitch = UnityEngine.Random.Range(.9f, 1.1f);
            audioSource.PlayOneShot(collectSound);
            GetComponent<SpriteRenderer>().enabled = false; 
            Invoke(nameof(Cleanup), 1f);
        }
    }

    void Cleanup()
    {
        Destroy(gameObject);
    }

}