using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonFadeOnSelect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    Material mat;
    [SerializeField] Image targetImage;
    [SerializeField] Button TargetButton;
    RectTransform rect;
    Vector3 originalScaling;
    [SerializeField] bool applyScalingEffect = true;
    [SerializeField] bool applyMouseoverEffects = true;

    [SerializeField] bool shouldReplaceWithLevelTheme = true;
    [SerializeField] Color colorToReplace = Color.white;  
    public Color replacementColor = Color.magenta;
    [SerializeField] TextMeshProUGUI textMesh;

    bool isSelected => TryGetComponent<Selectable>(out var selectable)
            && EventSystem.current.currentSelectedGameObject
            && selectable.gameObject == EventSystem.current.currentSelectedGameObject;

    private void OnEnable()
    {
        mat = Resources.Load<Material>("Materials/UIFade");

        if (shouldReplaceWithLevelTheme)
            replacementColor = PersistentData.Instance.CurrentLevelConfig.UIColor;

        if (textMesh != null)
            textMesh.color = replacementColor;

        if (targetImage == null)
            targetImage = GetComponent<Image>();

        if (targetImage == null)
        {
            this.enabled = false;
            return;
        }

        if (TargetButton == null && TryGetComponent<Button>(out TargetButton))
            TargetButton.colors = new ColorBlock()
            {
                normalColor = replacementColor,
                highlightedColor = Color.white,
                selectedColor = Color.white,
                pressedColor = Color.white,
                colorMultiplier = 1f
            };

        rect = GetComponent<RectTransform>();
        originalScaling = rect.localScale;

        if (targetImage == null && TargetButton != null)
            targetImage = TargetButton.GetComponent<Image>();

        targetImage.material = new Material(mat);
        targetImage.material.SetColor("_ColorToReplace", colorToReplace);
        targetImage.material.SetColor("_ReplacementColor", replacementColor);
        if (isSelected)
            DoSelection();
    }

    private void DoSelection()
    {
        if (!applyMouseoverEffects) return;

        targetImage.material.SetFloat("_Selected", 1f);
        targetImage.material.SetFloat("_SelectedTime", Time.time);
        if(applyScalingEffect)
            rect.localScale = originalScaling * 1.1f;
    }

    private void DoDeselection()
    {
        if (!applyMouseoverEffects) return;

        targetImage.material.SetFloat("_Selected", 0f);
        targetImage.material.SetFloat("_SelectedTime", Time.time);
        if (applyScalingEffect)
            rect.localScale = originalScaling;
    }
    public void OnDeselect(BaseEventData eventData) => DoDeselection();

    public void OnSelect(BaseEventData eventData) => DoSelection();

    public void OnPointerEnter(PointerEventData eventData) => DoSelection();

    public void OnPointerExit(PointerEventData eventData) => DoDeselection();
}
