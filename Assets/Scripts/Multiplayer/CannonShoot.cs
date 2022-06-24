using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CannonShoot : NetworkBehaviour
{
    [Header("Components")]
    public Transform _spawnPos;

    [Header("Prefabs")]
    public GameObject _cannonBall;

    [Header("Settings")]
    [SerializeField] private float _shootForce = 1000f;
    [SerializeField] private float _shootDelay = 4f;
    [SerializeField] private float _destroyDelay = 10f;

    private GameObject _ballInstance;

    // Start is called before the first frame update
    void Start()
    {
        if (isServer)
            StartCoroutine(ShootLoop());
    }


    private IEnumerator ShootLoop()
    {
        yield return new WaitForSeconds(_shootDelay);

        RpcSpawnCannonball();

        if (_ballInstance)
        {
            _ballInstance.GetComponent<Rigidbody>().isKinematic = false;
            _ballInstance.GetComponent<Rigidbody>().AddForce(_spawnPos.forward * _shootForce, ForceMode.Impulse);
        }

        StartCoroutine(ShootLoop());
    }

    [ClientRpc]
    private void RpcSpawnCannonball()
    {
        _ballInstance = Instantiate(_cannonBall, _spawnPos.position, _spawnPos.rotation);
        Destroy(_ballInstance, _destroyDelay);
        _ballInstance.GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<AudioSource>().Play();
    }
}
