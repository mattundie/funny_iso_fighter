using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerWeaponController : NetworkBehaviour
{
    [Header("Equipped Weapon:")]
    [SerializeField] private GameObject _equippedWeapon;

    [Header("Components")]
    [SerializeField] private Transform _equipLocation;

    private bool _hasWeapon = false;
    private PlayerStatusController _status;
    private PlayerMovementController _movement;

    // Start is called before the first frame update
    void Start()
    {
        _status = GetComponent<PlayerStatusController>();
        _movement = GetComponent<PlayerMovementController>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckForWeapon();
    }

    void CheckForWeapon() {
        // If you have a weapon already, or the interact object is not a weapon, return;
        if (_hasWeapon || _status._collisionDetection._interactObject.tag != "Weapon")
            return;

        if (_status._collisionDetection._interactAvailable) {
            if (_movement._input._interactPressed) {
                _equippedWeapon = _status._collisionDetection._interactObject;
                _equippedWeapon.GetComponent<Weapon>().Pickup(_equipLocation);
                _hasWeapon = true;
            }
        }
    }
}
