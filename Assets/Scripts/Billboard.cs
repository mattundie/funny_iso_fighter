using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform Camera;
    //private float _speed = 5;

    private void Start()
    {
        Camera = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }

    private void LateUpdate()
    {
        if (!Camera)
        {
            Camera = GameObject.FindGameObjectWithTag("MainCamera").transform;
        }
        else
            transform.LookAt(transform.position + Camera.forward);
    }
}
