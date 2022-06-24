using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
    [Header("Particle Systems")]
    public ParticleSystem _runParticleSystem;
    
    [Header("Prefabs")]
    public GameObject _bloodPrefab;

    [Header("Audio Clips")]
    public AudioClip _punchAudioClip;
    public AudioClip _contactAudioClip;
    public AudioClip _runAudioClip;
    public AudioClip _fishAudioClip;

    [Header("Tracked Data")]
    [SerializeField] private bool _bleeding = false;
    [SerializeField] private GameObject _bloodInstance;

    private AudioSource _playerAudio;

    private void Start()
    {
        _playerAudio = GetComponent<AudioSource>();
    }

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
            _bloodInstance.GetComponent<ParticleSystem>().Play();
            PlayAudio(_punchAudioClip);
            _bleeding = true;

            Destroy(_bloodInstance,_bloodInstance.GetComponent<ParticleSystem>().main.duration + 0.15f);
        }
    }

    public void PlayerFishSound()
    {
        PlayAudio(_fishAudioClip);
    }

    private void PlayAudio(AudioClip clip)
    {
        if (!_playerAudio.isPlaying)
        {
            _playerAudio.PlayOneShot(clip);
        }
    }
}
