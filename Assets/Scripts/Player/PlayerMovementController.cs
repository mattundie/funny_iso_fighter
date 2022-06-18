using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using UnityEngine.SceneManagement;

public class PlayerMovementController : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Transform _mouseTarget;
    [SerializeField] private GameObject _playerModel;
    [SerializeField] private GameObject _playerRigging;

    [Header("Settings")]
    [SerializeField] private float _moveSpeed = 6;
    [SerializeField] private float _slideSpeed = 5;
    [SerializeField] private float _rotateSpeed = 500f;
    [SerializeField] private bool _isMoving;
    [SerializeField] private bool _isSliding;
    [SerializeField] private string[] EnabledScenes = { "Game" };
    private Vector3 _input;
    private bool _enabled;

    private bool IsMouseOverGameWindow { get { return !(0 > Input.mousePosition.x || 0 > Input.mousePosition.y || Screen.width < Input.mousePosition.x || Screen.height < Input.mousePosition.y); } }

    private void Start() {
        _isMoving = false;
        _isSliding = false;
        _enabled = false;
        _playerModel.SetActive(false);
        _playerRigging.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        ValidateMovementPermissions();
        
        if(!_enabled) { return; }

        if (hasAuthority)
        {
            GatherInput();
            Rotate();
            Animate();
            Slide();

            if (_playerRigging.activeSelf == false)
                _playerRigging.SetActive(true);
        }
    }

    private void FixedUpdate()
    {
        if(hasAuthority)
            Move();
    }

    void Reposition(Vector3 location)
    {
        transform.position = location;
    }

    void ValidateMovementPermissions()
    {
        if (EnabledScenes.Contains(SceneManager.GetActiveScene().name))
        {
            if (!_enabled)
            {
                Reposition(new Vector3(Random.Range(-5, 5), 2f, Random.Range(-5, 5)));
                _playerModel.SetActive(true);
                _enabled = true;
            }
        }
    }

    void GatherInput()
    {
        _input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
    }

    #region Movement Functions
    void Move()
    {
        if (_isSliding) return;

        _rb.MovePosition(transform.position + _input.normalized.ToIso() * _moveSpeed * Time.deltaTime);
    }

    void Slide()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !_isSliding)
        {
            GetComponent<Animator>().SetTrigger("slide");
            _isSliding = true;

            Vector3 boostVector = transform.forward * _slideSpeed;
            _rb.velocity = _rb.velocity + boostVector;
        }

        if (!GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Slide"))
        {
            _isSliding = false;
        }
    }

    void Rotate() {

        if (_input == Vector3.zero) {
            _isMoving = false;
            return;
        }

        _isMoving = true;

        Vector3 movementDirection = _input.ToIso();
        movementDirection.Normalize();

        Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, _rotateSpeed * Time.deltaTime);
    }
    #endregion

    #region Animation
    void Animate() {
        GetComponent<Animator>().SetBool("moving", _isMoving);

        // Move head to follow mouse cursor
        if (IsMouseOverGameWindow && hasAuthority)
        {
            RaycastHit hit;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                _mouseTarget.position = new Vector3(hit.point.x, transform.position.y + 0.5f, hit.point.z);
            }
        }
    }
    #endregion
}

public static class Helpers
{
    private static Matrix4x4 _isoMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
    public static Vector3 ToIso(this Vector3 input) => _isoMatrix.MultiplyPoint3x4(input);
}

