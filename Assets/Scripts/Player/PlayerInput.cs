using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour {
    public KeyCode[] _jumpInput = { KeyCode.Space, KeyCode.JoystickButton0 };
    public KeyCode[] _actionInput = { KeyCode.Mouse1, KeyCode.JoystickButton1 };
    public KeyCode[] _interactInput = { KeyCode.E, KeyCode.JoystickButton1 };
    public Vector3 _moveInput = Vector3.zero;

    public bool _jumpPressed = false;
    public bool _actionPressed = false;
    public bool _movePressed = false;
    public bool _interactPressed = false;

    private float _jumpTimeout = 0.2f;
    private float _jumpCounter = 0f;

    private bool IsMouseOverGameWindow { get { return !(0 > Input.mousePosition.x || 0 > Input.mousePosition.y || Screen.width < Input.mousePosition.x || Screen.height < Input.mousePosition.y); } }

    public enum InputType {
        Down,
        Hold,
        Up
    }


    // Update is called once per frame
    void Update() {
        JumpDetection();
        ActionDetection();
        MoveDetection();
        InteractDetection();
        CursorController();
    }

    public bool IsJumpInput(InputType type) {
        if(type == InputType.Down) {
            foreach (var i in _jumpInput)
                if (Input.GetKeyDown(i))
                    return true;
            return false;
        }
        else if (type == InputType.Hold) {
            foreach (var i in _jumpInput)
                if (Input.GetKey(i))
                    return true;
            return false;
        }
        else if (type == InputType.Up) {
            foreach (var i in _jumpInput)
                if (Input.GetKeyUp(i))
                    return true;
            return false;
        }

        return false;
    }

    void CursorController()
    {
        if (IsMouseOverGameWindow)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    void JumpDetection() {
        foreach (var i in _jumpInput) {
            if (Input.GetKeyDown(i)) {
                _jumpPressed = true;
                _jumpCounter = Time.time;
            }
            else if (Input.GetKey(i)) {
                _jumpPressed = true;
                if (Time.time - _jumpCounter > _jumpTimeout)
                    _jumpPressed = false;
            }
            else if (Input.GetKeyUp(i)) {
                _jumpPressed = false;
                _jumpCounter = 0f;
            }
        }
    }

    public bool IsActionInput(InputType type) {
        if (type == InputType.Down) {
            foreach (var i in _actionInput)
                if (Input.GetKeyDown(i))
                    return true;
            return false;
        }
        else if (type == InputType.Hold) {
            foreach (var i in _actionInput)
                if (Input.GetKey(i))
                    return true;
            return false;
        }
        else if (type == InputType.Up) {
            foreach (var i in _actionInput)
                if (Input.GetKeyUp(i))
                    return true;
            return false;
        }

        return false;
    }

    void ActionDetection() {
        foreach (var i in _actionInput) {
            if (Input.GetKeyDown(i) || Input.GetKey(i))
                _actionPressed = true;
            else if (Input.GetKeyUp(i))
                _actionPressed = false;
        }
    }

    void InteractDetection() {
        foreach (var i in _interactInput) {
            if (Input.GetKeyDown(i) || Input.GetKey(i))
                _interactPressed = true;
            else if (Input.GetKeyUp(i))
                _interactPressed = false;
        }
    }

    void MoveDetection() {
        _moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        if (_moveInput != Vector3.zero)
            _movePressed = true;
        else
            _movePressed = false;
    }
}
