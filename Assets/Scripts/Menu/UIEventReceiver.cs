using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIEventReceiver : MonoBehaviour
{
    public InputActionReference action;
    public UnityEvent OnActionPerformed;

    private void OnEnable()
    {
        action.action.performed += PerformAction;
    }

    private void OnDisable()
    {
        action.action.performed -= PerformAction;
    }

    private void PerformAction(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            OnActionPerformed?.Invoke();
        }
    }

}
