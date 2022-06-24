using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
    [Header("Particle Systems")]
    public ParticleSystem _runParticleSystem;
    
    [Header("Prefabs")]
    public GameObject _bloodPrefab;

    [Header("Tracked Data")]
    [SerializeField] private bool _bleeding = false;
    [SerializeField] private GameObject _bloodInstance;

    private void Update()
    {
        if (_bleeding)
        {
            if (!_bloodInstance)
                _bleeding = false;
        }
    }

    public void CreateBloodEffect(Vector3 spawnLocation, GameObject attachTo)
    {
        if (!_bleeding)
        {
            _bloodInstance = Instantiate(_bloodPrefab, spawnLocation, Quaternion.identity);
            _bloodInstance.transform.parent = attachTo.transform;
            _bleeding = true;

            Destroy(_bloodInstance,_bloodInstance.GetComponent<ParticleSystem>().main.duration + 0.15f);
        }
    }
}
