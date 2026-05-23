using UnityEngine;
using UnityEngine.EventSystems;

namespace Menu
{
    public class UIButtonHoverSound : MonoBehaviour, IPointerEnterHandler
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip hoverSound;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            audioSource.PlayOneShot(hoverSound);
        }
    }
}