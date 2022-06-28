using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [Header("Info")]
    public string _name = "DefaultWeapon";
    public int _id = 0;
    public bool _enabled = true;
    public bool _acting = false;

    private void Start() {
        _acting = false;

        Preload();
    }

    private void Update() {
        Drop();
    }

    public abstract void Preload();
    public abstract void Pickup(Transform parent);
    public abstract void Action();
    public abstract void Drop();
}
