using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionDetection : MonoBehaviour
{
    [Header("Status Variables:")]
    public bool _interactAvailable = false;
    public GameObject _interactObject;


    private void OnTriggerEnter(Collider collision) {
        if(collision.transform.tag == "Weapon") {
            _interactAvailable = true;
            _interactObject = collision.gameObject;
        }
    }

    private void OnTriggerExit(Collider collision) {
        if(collision.transform.tag == "Weapon") {
            _interactAvailable = false;
            _interactObject = null;
        }
    }
}
