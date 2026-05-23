using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleWeapon : MonoBehaviour
{
    [SerializeField] private AudioClip collectSound;
    [SerializeField] int AmountToAdd;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (TryGetComponent<Animator>(out var animator))
        {
            animator.runtimeAnimatorController = Data.Instance.GetProjectileAnimatorComponentByCharacterName(PersistentData.Instance.CurrentCharacter.name);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent(out Player player))
        {
                GetComponent<BoxCollider2D>().enabled = false;
                player.AddWeapons(AmountToAdd);
                _audioSource.PlayOneShot(collectSound);
                GetComponent<SpriteRenderer>().enabled = false;
                Invoke(nameof(Cleanup), 1f);
        }
    }

    void Cleanup()
    {
        Destroy(gameObject);
    }
}
