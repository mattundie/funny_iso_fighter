using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.Animations.Rigging;
using RootMotion.Dynamics;
using RootMotion.FinalIK;
using System;

public class PlayerMovementController : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] public Rigidbody _rb;
    [SerializeField] private Animator _animator;
    [SerializeField] private GameObject _playerModel;
    [SerializeField] public PlayerInput _input;
    [SerializeField] public GameObject _puppet;
    [SerializeField] private BehaviourPuppet _puppetBehaviour;
    [SerializeField] private Transform _mouseTarget;
    [SerializeField] private Transform _raycastCenter;
    [SerializeField] private LayerMask _raycastLayers;
    [SerializeField] private ExplosiveContact[] _explosiveContacts;

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
    [SerializeField] private float _initialJumpForceMultiplier = 6f;
    [SerializeField] private float _continualJumpForceMultiplier = 1.25f;
    [SerializeField] private float _jumpTime = 0.2f;
    [SerializeField] private float _jumpTimeCounter = 0f;
    [SerializeField] private float _coyoteTime = 0.2f;
    [SerializeField] private float _coyoteTimeCounter = 0f;
    [SerializeField] private float _jumpBufferTime = 0.4f;
    [SerializeField] private float _jumpBufferTimeCounter = 0f;
    [SerializeField] private bool _jumpWasPressedLastFrame = false;

    [Header("Monitored Data")]
    [SerializeField] [SyncVar] public bool _isMoving = false;
    [SerializeField] [SyncVar] public bool _isJumping = false;
    [SerializeField] [SyncVar] public bool _isActing = false;
    [SerializeField] [SyncVar] public bool _grounded;
    [SerializeField] [SyncVar] private bool _enabled;
    [SerializeField] private ActionState _currentActionState;
    
    private PlayerStatusController _playerStatus;

    private enum ActionState
    {
        melee,
        shoot
    }

    private RaycastHit _raycastHit;

    private bool IsMouseOverGameWindow { get { return !(0 > Input.mousePosition.x || 0 > Input.mousePosition.y || Screen.width < Input.mousePosition.x || Screen.height < Input.mousePosition.y); } }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        _currentActionState = ActionState.melee;
        _playerStatus = this.GetComponent<PlayerStatusController>();

        DisablePlayer();

        if (!hasAuthority)
        {
            _rb.gameObject.GetComponent<AimIK>().enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!_enabled || !hasAuthority) { return; }

        Animate();

        if (_puppetBehaviour.state == BehaviourPuppet.State.Puppet)
        {
            Rotate();
            Action();
        }
    }

    private void FixedUpdate()
    {
        if (!_enabled || !hasAuthority) { return; }

        Jump();
        Hover();
        Move();
    }

    private void DisablePlayer()
    {
        _playerStatus.HealthBar.parent.gameObject.SetActive(false);
        _playerModel.SetActive(false);
        _puppet.SetActive(false);
        _rb.isKinematic = true;
        _enabled = false;
    }

    private void EnablePlayer()
    {
        _playerStatus.HealthBar.parent.gameObject.SetActive(true);
        _rb.isKinematic = false;
        _playerModel.SetActive(true);
        _puppet.SetActive(true);
        _enabled = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (!_enabled) {
            if (scene.name.Contains("Game")) {
                Vector3 spawnPos = this.GetComponent<PlayerObjectController>()._spawnPoint;
                _rb.transform.position = spawnPos;
                _puppet.transform.position = spawnPos;

                EnablePlayer();
            }
        }
        else {
            if (!scene.name.Contains("Game")) {
                DisablePlayer();
            }
        }
    }

    public void UpdatePlayerState(PlayerStatusController.PlayerState state)
    {
        if(state == PlayerStatusController.PlayerState.Respawn)
        {
            _puppet.SetActive(false);

            Vector3 spawnPos = this.GetComponent<PlayerObjectController>()._spawnPoint;
            _rb.transform.position = spawnPos;
            _puppet.transform.position = spawnPos;

            EnablePlayer();

            _puppet.GetComponent<PuppetMaster>().state = PuppetMaster.State.Alive;
        }
        else if (state == PlayerStatusController.PlayerState.Dead)
        {
            _rb.isKinematic = true;
            _enabled = false;

            _puppet.GetComponent<PuppetMaster>().state = PuppetMaster.State.Dead;
        }
        else if(state == PlayerStatusController.PlayerState.Alive)
        {
            _rb.isKinematic = false;
            _enabled = true;

            _puppet.GetComponent<PuppetMaster>().state = PuppetMaster.State.Alive;
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
            CmdIsGrounded(true);

            if (_puppetBehaviour.state != BehaviourPuppet.State.Puppet || _isJumping)
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
            CmdIsGrounded(false);
        }
    }

    void Move()
    {
        Vector3 groundVel;
        Vector3 move = _input._moveInput.normalized.ToIso();
        Vector3 m_UnitGoal = move;
        Vector3 m_GoalVel = _rb.velocity;
        Vector3 unitVel = m_GoalVel.normalized;

        if(_puppetBehaviour.state != BehaviourPuppet.State.Puppet)
        {
            _isMoving = false;
            CmdIsMoving(false);
            return;
        }

        if (move == Vector3.zero)
        {
            if (_grounded)
                _rb.velocity = _rb.velocity * 0.9f;

            _isMoving = false;
            CmdIsMoving(false);
            return;
        }

        _isMoving = true;
        CmdIsMoving(true);

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
        if (_input._moveInput == Vector3.zero)
        {
            _rb.angularVelocity = _rb.angularVelocity * 0.05f;
            return;
        }

        Vector3 movementDirection = _input._moveInput.ToIso();
        movementDirection.Normalize();

        Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
        _rb.transform.rotation = Quaternion.RotateTowards(_rb.transform.rotation, toRotation, _rotateSpeed * Time.deltaTime);
    }

    void Jump()
    {
        float calculatedJumpInput;

        SetJumpTimeCounter();
        SetCoyoteTimeCounter();
        SetJumpBufferCounter();

        if (_puppetBehaviour.state != BehaviourPuppet.State.Puppet)
            return;

        if (_input._jumpPressed && _jumpBufferTimeCounter > 0.0f && !_isJumping && _coyoteTimeCounter > 0.0f)
        {
            calculatedJumpInput = _initialJumpForceMultiplier;
            _rb.AddForce(new Vector3(0, calculatedJumpInput, 0), ForceMode.Impulse);

            _animator.SetTrigger("jump");
            _jumpBufferTimeCounter = 0;
            _coyoteTimeCounter = 0;
            _isJumping = true;
        }
        else if (_input._jumpPressed && _isJumping && !_grounded && _jumpTimeCounter > 0.0f)
        {
            calculatedJumpInput = _initialJumpForceMultiplier * _continualJumpForceMultiplier;
            _rb.AddForce(new Vector3(0, calculatedJumpInput, 0), ForceMode.Force);
        }
        else if(_isJumping && _grounded && !_input._jumpPressed) {
            _isJumping = false;
        }
    }

    void Action()
    {
        if (_puppetBehaviour.state != BehaviourPuppet.State.Puppet)
            return;

        // Begin Action
        if (_input.IsActionInput(PlayerInput.InputType.Down))
        {
            if (!_isActing && _currentActionState == ActionState.melee)
            {
                CmdPlayerAction();
            }
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

        if (IsMouseOverGameWindow)
        {
            RaycastHit hit;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, _raycastLayers))
            {
                _mouseTarget.position = new Vector3(hit.point.x, _rb.transform.position.y, hit.point.z);

                float dist = (_mouseTarget.transform.position - _rb.transform.position).magnitude;
                if (dist <= 1 && dist > 0)
                {
                    _rb.gameObject.GetComponent<AimIK>().solver.IKPositionWeight = dist;
                }
                else
                {
                    _rb.gameObject.GetComponent<AimIK>().solver.IKPositionWeight = 1;
                }
            }
        }
    }
    #endregion

    #region ClientRpc Functions
    [ClientRpc]
    void RpcPlayerAction()
    {
        // Call Command Function
        float duration = 1f;

        _animator.SetTrigger("slap");
        foreach (var contact in _explosiveContacts)
            contact._enabled = true;

        Invoke("ActionReset", duration);
        Invoke("ExplosiveContactDisable", duration);
    }


    #endregion

    #region Command Functions
    [Command]
    void CmdPlayerAction()
    {
        _isActing = true;

        RpcPlayerAction();
    }

    [Command]
    void CmdIsMoving(bool moving)
    {
        _isMoving = moving;
    }

    [Command]
    void CmdIsGrounded(bool grounded)
    {
        _grounded = grounded;
    }
    #endregion

    #region Public Networking Functions

    #endregion

    #region Helper Functions
    private void ActionReset()
    {
        _isActing = false;
    }

    private void ExplosiveContactDisable()
    {
        foreach (var contact in _explosiveContacts)
            contact._enabled = false;
    }

    private void SetJumpTimeCounter()
    {
        if (_isJumping && !_grounded)
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
        if (!_jumpWasPressedLastFrame && _input._jumpPressed)
        {
            _jumpBufferTimeCounter = _jumpBufferTime;
        }
        else if (_jumpBufferTime > 0.0f)
        {
            _jumpBufferTimeCounter -= Time.fixedDeltaTime;
        }
        _jumpWasPressedLastFrame = _input._jumpPressed;
    }
    #endregion
}


public static class Helpers {
    private static Matrix4x4 _isoMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
    public static Vector3 ToIso(this Vector3 input) => _isoMatrix.MultiplyPoint3x4(input);
}