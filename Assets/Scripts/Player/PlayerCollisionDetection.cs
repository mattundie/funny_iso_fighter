using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionDetection : MonoBehaviour
{
    [Header("Status Variables:")]
    public bool InteractAvailable = false;
    public GameObject InteractObject;


    private void OnTriggerEnter(Collider collision) {
        if(collision.transform.CompareTag("Weapon")) {
            InteractAvailable = true;
            InteractObject = collision.gameObject;
        }
    }

    private void OnTriggerExit(Collider collision) {
        if(collision.transform.CompareTag("Weapon")) {
            InteractAvailable = false;
            InteractObject = null;
        }
    }
}
