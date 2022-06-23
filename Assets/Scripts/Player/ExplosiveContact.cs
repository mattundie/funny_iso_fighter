using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveContact : MonoBehaviour
{
    public bool _enabled = false;
    public float _explosiveForce = 100f;
    public float _damage = -15f;
    public PlayerMovementController _networkParent;

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
                _networkParent.ApplyExplosiveForce(collision, _explosiveForce, this.GetComponent<Rigidbody>().velocity, _damage);
            }
        }
    }
}
