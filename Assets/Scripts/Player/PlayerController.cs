using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Transform _mouseTarget;

    [Header("Settings")]
    [SerializeField] private float _moveSpeed = 6;
    [SerializeField] private float _slideSpeed = 5;
    [SerializeField] private float _rotateSpeed = 500f;
    [SerializeField] private bool _isMoving;
    [SerializeField] private bool _isSliding;
    private Vector3 _input;

    private bool IsMouseOverGameWindow { get { return !(0 > Input.mousePosition.x || 0 > Input.mousePosition.y || Screen.width < Input.mousePosition.x || Screen.height < Input.mousePosition.y); } }

    private void Start() {
        _isMoving = false;
        _isSliding = false;
    }

    // Update is called once per frame
    void Update()
    {
        GatherInput();
        Rotate();
        Animate();
        Slide();
    }

    private void FixedUpdate()
    {
        Move();
    }

    void GatherInput()
    {
        _input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
    }

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

    void Animate() {
        GetComponent<Animator>().SetBool("moving", _isMoving);

        // Move head to follow mouse cursor
        if (IsMouseOverGameWindow)
        {
            RaycastHit hit;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                _mouseTarget.position = new Vector3(hit.point.x, transform.position.y + 0.5f, hit.point.z);
            }
        }
    }
}

public static class Helpers
{
    private static Matrix4x4 _isoMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
    public static Vector3 ToIso(this Vector3 input) => _isoMatrix.MultiplyPoint3x4(input);
}

