using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.Animations.Rigging;
using RootMotion.Dynamics;
using System;

public class PlayerMovementNew : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Animator _animator;
    [SerializeField] private BehaviourPuppet _puppet;
    [SerializeField] private Transform _raycastCenter;
    [SerializeField] private LayerMask _raycastLayers;

    [Header("Move Settings")]
    [SerializeField] private float _maxSpeed = 8f;
    [SerializeField] private float _acceleration = 200f;
    [SerializeField] private AnimationCurve _accelerationFactorFromDot;
    [SerializeField] private float _maxAccelForce = 400f;
    [SerializeField] private AnimationCurve _maxAccelerationForceFactorFromDot;
    [SerializeField] private Vector3 _forceScale = new Vector3(1, 0, 1);

    [Header("Hover Settings")]
    [SerializeField] private float _rotateSpeed = 500f;
    [SerializeField] private float _rideHeight = 0.9f;
    [SerializeField] private float _rideSpringStrength = 300f;
    [SerializeField] private float _rideSpringDamper = 50f;

    [Header("Jump Settings")]
    [SerializeField] private float _initialJumpForceMultiplier = 750f;
    [SerializeField] private float _continualJumpForceMultiplier = 0.1f;
    [SerializeField] private float _jumpTime = 0.175f;
    [SerializeField] private float _jumpTimeCounter = 0f;
    [SerializeField] private float _coyoteTime = 0.15f;
    [SerializeField] private float _coyoteTimeCounter = 0f;
    [SerializeField] private float _jumpBufferTime = 0.2f;
    [SerializeField] private float _jumpBufferTimeCounter = 0f;
    [SerializeField] private bool _jumpWasPressedLastFrame = false;

    [Header("Monitored Data")]
    [SerializeField] private bool _isMoving = false;
    [SerializeField] private bool _isJumping = false;
    [SerializeField] private bool _grounded;

    private Vector3 _buttonInput;
    private RaycastHit _raycastHit;
    private bool _pressedJump;
    private float _pressedJumpTimer;

    private bool IsMouseOverGameWindow { get { return !(0 > Input.mousePosition.x || 0 > Input.mousePosition.y || Screen.width < Input.mousePosition.x || Screen.height < Input.mousePosition.y); } }

    private void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        GatherInput();

        if (_puppet.state == BehaviourPuppet.State.Puppet)
        {
            Rotate();
        }

        Animate();
    }

    private void FixedUpdate()
    {
        Jump();
        Hover();
        Move();
    }

    void GatherInput()
    {
        _buttonInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        if (Input.GetButtonDown("Jump"))
        {
            _pressedJump = true;
            _pressedJumpTimer = Time.time;
        }
        else if (Input.GetButton("Jump"))
        {
            _pressedJump = true;

            if (Time.time - _pressedJumpTimer > _jumpTime)
                _pressedJump = false;
        }
        if (Input.GetButtonUp("Jump"))
        {
            _pressedJump = false;
            _pressedJumpTimer = 0f;
        }
    }

    #region Movement Functions
    void Hover()
    {
        bool didRaycastHit = Physics.Raycast(_raycastCenter.position, Vector3.down, out _raycastHit, _rideHeight + 0.2f, _raycastLayers);
        Debug.DrawRay(_raycastCenter.position, Vector3.down * _rideHeight, Color.red);

        if (didRaycastHit)
        {
            _grounded = true;

            if (_puppet.state != BehaviourPuppet.State.Puppet || _isJumping)
                return;

            Vector3 vel = _rb.velocity;
            Vector3 rayDir = transform.TransformDirection(Vector3.down);

            Vector3 hitVel = Vector3.zero;
            Rigidbody hitBody = _raycastHit.rigidbody;
            if (hitBody != null)
            {
                hitVel = hitBody.velocity;
            }

            float rayDirVel = Vector3.Dot(rayDir, vel);
            float otherDirVel = Vector3.Dot(rayDir, hitVel);

            float relVel = rayDirVel - otherDirVel;

            float x = _raycastHit.distance - _rideHeight;

            float springForce = (x * _rideSpringStrength) - (relVel * _rideSpringDamper);

            _rb.AddForce(rayDir * springForce);

            if (hitBody != null)
            {
                hitBody.AddForceAtPosition(rayDir * -springForce, _raycastHit.point);
            }
        }
        else
        {
            _grounded = false;
        }
    }

    void Move()
    {
        Vector3 groundVel;
        Vector3 move = _buttonInput.normalized.ToIso();
        Vector3 m_UnitGoal = move;
        Vector3 m_GoalVel = _rb.velocity;
        Vector3 unitVel = m_GoalVel.normalized;

        if(_puppet.state != BehaviourPuppet.State.Puppet)
        {
            _isMoving = false;
            return;
        }

        if (move == Vector3.zero)
        {
            if (_grounded)
                _rb.velocity = _rb.velocity * 0.9f;

            _isMoving = false;
            return;
        }

        _isMoving = true;

        if (_raycastHit.rigidbody != null)
            groundVel = _raycastHit.rigidbody.velocity;
        else
            groundVel = Vector3.zero;

        float velDot = Vector3.Dot(m_UnitGoal, unitVel);

        float accel = _acceleration * _accelerationFactorFromDot.Evaluate(velDot);

        Vector3 goalVel = m_UnitGoal * _maxSpeed;

        m_GoalVel = Vector3.MoveTowards(m_GoalVel, (goalVel) + (groundVel), accel);

        Vector3 neededAccel = m_GoalVel - _rb.velocity;

        float maxAccel = _maxAccelForce * _maxAccelerationForceFactorFromDot.Evaluate(velDot);

        neededAccel = Vector3.ClampMagnitude(neededAccel, maxAccel);

        _rb.AddForce(Vector3.Scale(neededAccel * _rb.mass, _forceScale));
    }

    void Rotate()
    {

        if (_buttonInput == Vector3.zero)
        {
            _rb.angularVelocity = _rb.angularVelocity * 0.05f;
            return;
        }

        Vector3 movementDirection = _buttonInput.ToIso();
        movementDirection.Normalize();

        Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, _rotateSpeed * Time.deltaTime);
    }

    void Jump()
    {
        float calculatedJumpInput;

        SetJumpTimeCounter();
        SetCoyoteTimeCounter();
        SetJumpBufferCounter();

        if (_pressedJump && _jumpBufferTimeCounter > 0.0f && !_isJumping && _coyoteTimeCounter > 0.0f)
        {
            calculatedJumpInput = _initialJumpForceMultiplier;
            _rb.AddForce(new Vector3(0, calculatedJumpInput, 0), ForceMode.Impulse);

            _animator.SetTrigger("jump");
            _jumpBufferTimeCounter = 0;
            _coyoteTimeCounter = 0;
            _isJumping = true;
        }
        else if (_pressedJump && _isJumping && !_grounded && _jumpTimeCounter > 0.0f)
        {
            calculatedJumpInput = _initialJumpForceMultiplier * _continualJumpForceMultiplier;
            _rb.AddForce(new Vector3(0, calculatedJumpInput, 0), ForceMode.Force);
        }
        else if(_isJumping && _grounded && !_pressedJump)
        {
            _isJumping = false;
        }
    }

    #endregion

    #region Animation
    void Animate()
    {
        _animator.SetBool("moving", _isMoving);
        _animator.SetBool("grounded", _grounded);

        float clampedVelocity = _rb.velocity.magnitude / _maxSpeed;
        _animator.SetFloat("speed", clampedVelocity);
    }
    #endregion

    #region Helper Functions
    private void SetJumpTimeCounter()
    {
        if(_isJumping && !_grounded)
        {
            _jumpTimeCounter -= Time.fixedDeltaTime;
        }
        else
        {
            _jumpTimeCounter = _jumpTime;
        }
    }
    private void SetCoyoteTimeCounter()
    {
        if (_grounded)
        {
            _coyoteTimeCounter = _coyoteTime;
        }
        else
        {
            _coyoteTimeCounter -= Time.fixedDeltaTime;
        }
    }
    private void SetJumpBufferCounter()
    {
        if(!_jumpWasPressedLastFrame && _pressedJump)
        {
            _jumpBufferTimeCounter = _jumpBufferTime;
        }
        else if(_jumpBufferTime > 0.0f)
        {
            _jumpBufferTimeCounter -= Time.fixedDeltaTime;
        }
        _jumpWasPressedLastFrame = _pressedJump;
    }
    #endregion
}

