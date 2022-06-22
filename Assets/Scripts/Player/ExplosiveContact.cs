using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveContact : MonoBehaviour
{
    public bool _enabled = false;
    public float _explosiveForce = 100f;

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_enabled)
        {
            if (collision.rigidbody != null)
            {
                collision.rigidbody.AddForce(this.GetComponent<Rigidbody>().velocity * _explosiveForce, ForceMode.Impulse);
            }
        }
    }
}
