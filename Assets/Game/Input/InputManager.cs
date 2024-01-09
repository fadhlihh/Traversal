using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public Action<Vector3> OnMoveInput;
    public Action<bool> OnSprintInput;
    public Action OnPOVInput;
    public Action OnCrouchInput;
    public Action OnJumpInput;
    public Action OnClimbInput;
    public Action OnCancelClimb;
    public Action OnCancelGlide;
    public Action OnGlideInput;
    public Action OnPunchInput;

    [SerializeField]
    private bool _isToggleCrouch;

    private void Update()
    {
        CheckMovementInput();
        CheckSprintInput();
        CheckPOVInput();
        CheckCrouchInput();
        CheckJumpInput();
        CheckClimbInput();
        CheckCancelInput();
        CheckGlideInput();
        CheckPunchInput();
    }

    private void CheckCrouchInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            OnCrouchInput();
        }
        if (_isToggleCrouch)
        {
            if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                OnCrouchInput();
            }
        }
    }

    private void CheckMovementInput()
    {
        float horizontalAxis = Input.GetAxis("Horizontal");
        float verticalAxis = Input.GetAxis("Vertical");
        Vector3 axis = new Vector3(horizontalAxis, 0, verticalAxis);
        OnMoveInput(axis);
    }

    private void CheckSprintInput()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            OnSprintInput(true);
        }
        else
        {
            OnSprintInput(false);
        }
    }

    private void CheckPOVInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            OnPOVInput();
        }
    }

    private void CheckJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnJumpInput();
        }
    }

    private void CheckClimbInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            OnClimbInput();
        }
    }

    private void CheckCancelInput()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            OnCancelClimb();
            OnCancelGlide();
        }
    }

    private void CheckGlideInput()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            OnGlideInput();
        }
    }

    private void CheckPunchInput()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            OnPunchInput();
        }
    }
}
