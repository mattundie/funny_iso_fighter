using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private float _followSpeed;
    [SerializeField] private float _offsetY;

    private void Start()
    {
        target = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (!target)
        {
            target = GameObject.FindGameObjectWithTag("LocalPlayer");
        }
        else
        {
            if (transform.position != new Vector3(target.transform.position.x, target.transform.position.y + _offsetY, target.transform.position.z))
            {
                // Add target offset
                transform.position = Vector3.Lerp(transform.position, new Vector3(target.transform.position.x,
                                                                                  target.transform.position.y + _offsetY,
                                                                                  target.transform.position.z),
                                                                                  _followSpeed * Time.deltaTime);
            }
        }
    }
}
