using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FirstButtonSelector : MonoBehaviour
{
    [SerializeField] Selectable selectable;
    private void OnEnable()
    {
        if (InputManager.Instance?.PlayerInput?.currentControlScheme == "Keyboard&Mouse")
            return;

        EventSystem.current.SetSelectedGameObject(selectable.gameObject);
    }
}
