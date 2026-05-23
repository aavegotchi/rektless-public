using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorSwapper : MonoBehaviour
{
    [SerializeField] Image targetImage;
    [SerializeField] TextMeshProUGUI textMesh;

    private void OnEnable()
    {
        if (TryGetComponent<Image>(out targetImage))
        {
            targetImage.color = PersistentData.Instance.CurrentLevelConfig.UIColor;
        }
        if (TryGetComponent<TextMeshProUGUI>(out textMesh))
        {
            textMesh.color = PersistentData.Instance.CurrentLevelConfig.UIColor;
        }
    }
}
