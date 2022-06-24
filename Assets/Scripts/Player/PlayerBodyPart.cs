using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBodyPart : MonoBehaviour
{
    private GameObject _root;

    private void Start()
    {
        _root = transform.root.gameObject;
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckForPlayerActionContact(collision);
    }

    void CheckForPlayerActionContact(Collision collision)
    {
        if (collision.gameObject.layer == 7) // Ragdoll layer
        {
            ExplosiveContact contact = collision.gameObject.GetComponent<ExplosiveContact>();
            PlayerStatusController status = _root.GetComponent<PlayerStatusController>();

            if (contact)
                if (contact._enabled)
                    if (!status._dazed)
                    {
                        status.PlayerActionContact(this.gameObject, collision.rigidbody.velocity, contact._explosiveForce, contact._damage);
                    }
        }
    }
}