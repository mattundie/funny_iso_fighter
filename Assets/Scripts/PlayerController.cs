using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private Rigidbody _rb;
    [SerializeField] private float _speed = 6;
    [SerializeField] private float _rotateSpeed = 500f;
    [SerializeField] private bool _isMoving;
    private Vector3 _input;

    private bool IsMouseOverGameWindow { get { return !(0 > Input.mousePosition.x || 0 > Input.mousePosition.y || Screen.width < Input.mousePosition.x || Screen.height < Input.mousePosition.y); } }

    private void Start() {
        _isMoving = false;
    }

    // Update is called once per frame
    void Update()
    {
        GatherInput();
        Rotate();
        Animate();
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
        _rb.MovePosition(transform.position + _input.normalized.ToIso() * _speed * Time.deltaTime);
    }

    void Rotate() {

        /*if (IsMouseOverGameWindow)
        {
            RaycastHit hit;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 target = new Vector3(hit.point.x, transform.position.y, hit.point.z);
                Vector3 targetDirection = (target - transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotateSpeed * Time.deltaTime);
            }
        }*/

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
    }
}

public static class Helpers
{
    private static Matrix4x4 _isoMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
    public static Vector3 ToIso(this Vector3 input) => _isoMatrix.MultiplyPoint3x4(input);
}

