using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [Header("Info")]
    public string _name = "DefaultWeapon";
    public int _id = 0;
    public bool _enabled = false;
    public bool _acting = false;
    public bool _held = false;

    public abstract void Preload();
    public abstract void Action();
    public abstract void Drop();

    public void Pickup(Transform parent) {
        transform.position = parent.position;
        transform.rotation = parent.rotation;
        transform.parent = parent;
        gameObject.layer = parent.gameObject.layer;
        _held = true;

        GetComponent<Rigidbody>().isKinematic = true;

        transform.Find("UI").gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == 6 && !_held) {  // If collides with the ragdoll layer
            transform.Find("UI").gameObject.SetActive(true);
            enabled = true;
        }
    }
}
