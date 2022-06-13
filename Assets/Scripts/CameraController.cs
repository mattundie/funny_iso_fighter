using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float _followSpeed;
    [SerializeField] private float _offsetY;

    // Update is called once per frame
    void Update()
    {
        if (transform.position != new Vector3(target.position.x, target.position.y + _offsetY, target.position.z))
        {
            // Add target offset
            transform.position = Vector3.Lerp(transform.position, new Vector3(target.position.x,
                                                                              target.position.y + _offsetY,
                                                                              target.position.z),
                                                                              _followSpeed * Time.deltaTime);
        }
    }
}
