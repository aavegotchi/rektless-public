using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviourSingletonPersistent<InputManager>
{
    [SerializeField]
    PlayerInput playerInput;
    public PlayerInput PlayerInput => playerInput;

    InputAction move;
    InputAction jump;
    InputAction melee;
    InputAction fire;
    InputAction dash;
    InputAction pause;

    public Vector2 MoveInput;
    public Vector2Int MoveInputInt;
    public float lastJumpPressTime = -1f;
    public bool jumpPressed;
    public bool meleePressed;
    public bool firePressed;
    public bool dashPressed;

    public bool meleePressedThisFrame;
    public bool firePressedThisFrame;
    public bool dashPressedThisFrame;
    public bool dashReleasedThisFrame;

    public static Action OnPause;


    public override void Awake()
    {
        base.Awake();

        move = playerInput.actions["Move"];
        jump = playerInput.actions["Jump"];
        melee = playerInput.actions["Melee"];
        fire = playerInput.actions["Fire"];
        dash = playerInput.actions["Dash"];
        pause = playerInput.actions["Pause"];

        jump.performed += Jump;
        fire.performed += Fire;
        melee.performed += Melee;
        dash.performed += Dash;
        dash.canceled += Dash;
        jump.canceled += Jump;
        fire.canceled += Fire;
        melee.canceled += Melee;
        pause.performed += Pause;

    }

    private void Update()
    {
        MoveInput = move.ReadValue<Vector2>();

        MoveInputInt.x = Mathf.RoundToInt(MoveInput.x);
        MoveInputInt.y = Mathf.RoundToInt(MoveInput.y);
    }

    private void LateUpdate()
    {
        meleePressedThisFrame = false;
        firePressedThisFrame = false;
        dashPressedThisFrame = false;
        dashReleasedThisFrame = false;
    }

    private void OnDestroy()
    {
        jump.performed -= Jump;
        fire.performed -= Fire;
        melee.performed -= Melee;
        dash.performed -= Dash;
        dash.canceled -= Dash;
        jump.canceled -= Jump;
        fire.canceled -= Fire;
        melee.canceled -= Melee;

    }


    private void Jump(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed)
        {
            jumpPressed = true;
            lastJumpPressTime = Time.time;
        }
        else if (ctx.phase == InputActionPhase.Canceled)
            jumpPressed = false;
    }

    private void Fire(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed)
        {
            firePressed = true;
            firePressedThisFrame = true;
        }
        else if (ctx.phase == InputActionPhase.Canceled)
            firePressed = false;
    }

    private void Melee(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed)
        {
            meleePressed = true;
            meleePressedThisFrame = true;
        }
        else if (ctx.phase == InputActionPhase.Canceled)
            meleePressed = false;
    }

    private void Dash(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed)
        {
            dashPressed = true;
            dashPressedThisFrame = true;
        }
        else if (ctx.phase == InputActionPhase.Canceled)
        {
            dashPressed = false;
            dashReleasedThisFrame = true;
        }
    }

    private void Pause(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed)
            OnPause?.Invoke();
    }
}
