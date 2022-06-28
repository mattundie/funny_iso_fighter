using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlizzyGun : Weapon
{
    [Header("Components")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _spawnPoint;

    [Header("Configs")]
    [SerializeField] private float _cooldownTime = 0.5f;
    [SerializeField] private float _projectileForce = 10f;

    [Header("Static Info")]
    [SerializeField] private static int _maxAmmo = 4;
    [SerializeField] private int _shotsFired = 0;
    [SerializeField] private GameObject[] _magazine;

    private Rigidbody _rb;

    private void Start() {
        _rb = this.GetComponent<Rigidbody>();
    }

    public override void Preload() {
        _magazine = new GameObject[_maxAmmo];

        for(int i=0; i < _maxAmmo; i++) {
            _magazine[i] = GameObject.Instantiate(_projectilePrefab, this.transform.position, _spawnPoint.rotation);
            _magazine[i].SetActive(false);
        }

        Debug.Log($"Preloaded {this.name}");
    }

    public override void Pickup(Transform parent) {
        transform.position = parent.position;
        transform.rotation = parent.rotation;
        transform.parent = parent;
    }

    public override void Action() {
        if (!_acting) {
            _magazine[_shotsFired].transform.position = _spawnPoint.position;
            _magazine[_shotsFired].GetComponent<Rigidbody>().AddForce(_spawnPoint.forward * _projectileForce, ForceMode.Impulse);

            _shotsFired += 1;
            _acting = true;
            StartCoroutine(ActionCooldown());
        }
    }

    public override void Drop() {
        if(_shotsFired == _maxAmmo) {
            transform.parent = null;
            _rb.isKinematic = false;
            _enabled = false;
        }
    }

    private IEnumerator ActionCooldown() {
        yield return new WaitForSeconds(_cooldownTime);
        _acting = false;
    }
}
