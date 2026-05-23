using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIDisplayNameInput : MonoBehaviour
{
    [SerializeField] TMP_InputField InputField;
    public void UpdateDisplayName()
    {
        if (InputField.text == null || InputField.text.Length == 0 || InputField.text == string.Empty)
            return;

        PlayfabManager.Instance.UpdateName(InputField.text);
    }
}
