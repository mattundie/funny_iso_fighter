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

    public abstract void Preload();
    public abstract void Action();
    public abstract void Drop();

    public void Pickup(Transform parent) {
        transform.position = parent.position;
        transform.rotation = parent.rotation;
        transform.parent = parent;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == 7) {  // If collides with the ragdoll layer
            transform.Find("UI").gameObject.SetActive(true);
            enabled = true;
        }
    }
}
