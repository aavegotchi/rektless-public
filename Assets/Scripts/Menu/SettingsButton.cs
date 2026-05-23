using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Menu
{
    public class SettingsButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] public Color defaultColor;
        [SerializeField] private Color hoverColor;

        [SerializeField] private TextMeshProUGUI text;

        private void Start()
        {
            text.color = defaultColor;
        }

        private void OnEnable()
        {
            text.color = defaultColor;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            text.color = hoverColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            text.color = defaultColor;
        }

        public void OnSelect(BaseEventData eventData)
        {
            text.color = hoverColor;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            text.color = defaultColor;
        }
    }
}