using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectedButtonFinder : MonoBehaviour
{
    private void Update()
    {
        if (InputManager.Instance.PlayerInput.currentControlScheme == "Keyboard&Mouse")
            return;


        if (EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.activeInHierarchy)
            return;

        Selectable selectable = gameObject.GetComponentInChildren<Selectable>(false);

        if (selectable != null)
            EventSystem.current.SetSelectedGameObject(selectable.gameObject);
    }
}
