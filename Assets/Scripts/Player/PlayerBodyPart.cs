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
        PlayerStatusController status = _root.GetComponent<PlayerStatusController>();

        if (collision.gameObject.layer == 7 || collision.gameObject.layer == 9) // If ragdoll or collision object play noise
        {
            if (GetComponent<AudioSource>())
                GetComponent<AudioSource>().Play();

            if (collision.transform.tag == "Projectile")
                status.ModifyHealth(-1f);
        }

    }
}
