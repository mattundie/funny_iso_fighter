using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private Rigidbody _rb;
    [SerializeField] private float _speed = 6;
    private float _rotateSpeed = 500f;
    private Vector3 _input;

    // Extension stuff for iso specific movement
    private Matrix4x4 _isoMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
    public Vector3 ToIso(Vector3 input) => _isoMatrix.MultiplyPoint3x4(input);

    // Update is called once per frame
    void Update()
    {
        GatherInput();
        Rotate();
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
        _rb.MovePosition(transform.position + ToIso(_input) * _speed * Time.deltaTime);
    }

    void Rotate()
    {
        if (_input == Vector3.zero) return;

        Vector3 movementDirection = ToIso(_input);
        movementDirection.Normalize();

        Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, _rotateSpeed * Time.deltaTime);
    }
}

