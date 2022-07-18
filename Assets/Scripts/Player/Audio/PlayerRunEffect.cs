using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunEffect : MonoBehaviour
{
    public Animator _playerAnimator;
    [SerializeField] private bool _playing = false;
    private AudioSource _runSource;

    private void Start()
    {
        _runSource = this.GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!_playerAnimator) return;

        if (_playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Locomotion"))
        {
            if (_playing)
            {
                float speed = _playerAnimator.GetFloat(Animator.StringToHash("speed"));
                float modifier = 1 + speed / 4;
                _runSource.pitch = modifier;
            }
            else
            {
                _runSource.Play();
                _playing = true;
            }
        }
        else
        {
            if (_playing)
            {
                _runSource.Stop();
                _playing = false;
            }
        }
    }
}
