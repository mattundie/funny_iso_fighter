using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Steamworks;
using RootMotion.Dynamics;

public class PlayerStatusController : NetworkBehaviour
{
    #region Components
    [Header("Components")]
    public Transform HealthBar;
    public PlayerAvatar _avatar;
    public PlayerEffects _effects;
    #endregion

    #region Status Data
    [Header("Status Data")]
    [SyncVar] public float _health;
    [SyncVar] public float _maxHealth = 100;
    [SyncVar] public bool _dead = false;
    [SyncVar] public bool _dazed = false;
    [SyncVar] public int _deaths;
    #endregion

    #region Settings
    [SerializeField] private float _dazeDuration = 2f;
    #endregion

    private PlayerMovementController _movement;
    private float _dazedTimeStart = 0f;

    public enum PlayerState
    {
        Alive,
        Dead,
        Respawn
    }

    private Vector3 _healthBarTarget;

    private void Start()
    {
        _health = _maxHealth;
        _movement = GetComponent<PlayerMovementController>();
    }

    private void Update()
    {
        PlayerRunEffects();

        PlayerUndazed();

        if (_health == 0 && !_dead)
            PlayerDeath();

        if (!hasAuthority) { return; }

        if (Input.GetKeyDown(KeyCode.R) && _dead)
            CmdPlayerRespawn();
    }

    private void FixedUpdate()
    {
        PopulateHealthUI();

        if(!hasAuthority) { return; }

        if (_movement._rb.transform.position.magnitude > 150f)
        {
            PlayerDeath();
            CmdPlayerRespawn();
        }
    }

    private void PopulateHealthUI()
    {
        if (_health < 35)
        {
            HealthBar.gameObject.GetComponent<Image>().color = Color.red;
        }
        else if (_health >= 35)
        {
            HealthBar.gameObject.GetComponent<Image>().color = Color.green;
        }
    }

    private void PlayerRunEffects()
    {
        if (_movement._isMoving)
        {
            if (_movement._grounded)
            {
                if (!_effects._runParticleSystem.isPlaying || _effects._runParticleSystem.isStopped)
                {
                    _effects._runParticleSystem.Play();
                }
            }
            else
            {
                _effects._runParticleSystem.Stop();
            }
        }
        else
        {
            _effects._runParticleSystem.Stop();
        }
    }

    public void ModifyHealth(float modifier)
    {
        if (!isServer) return;

        _health += modifier;

        if (_health >= 0 && _health <= _maxHealth)
            RpcHealthChanged(_health);
        else if (_health > _maxHealth)
            RpcHealthChanged(_maxHealth);
        else if (_health < 0)
            RpcHealthChanged(0);
    }

    public void PlayerDeath()
    {
        if (!isServer) { return; }

        _dead = true;
        _deaths += 1;

        RpcPlayerDeath();
    }

    public void PlayerActionContact(GameObject bodyPart, Vector3 velocity, float force, float damage)
    {
        if (!_dazed)
        {
            _dazedTimeStart = Time.time;

            if (isServer)
            {
                _dazed = true;

                ModifyHealth(damage);
                RpcPlayerDazed();
            }

            if (hasAuthority)
            {
                bodyPart.GetComponent<Rigidbody>().AddForce(velocity * force, ForceMode.Impulse);
            }
        }
    }

    #region ClientRpc Calls

    [ClientRpc]
    private void RpcHealthChanged(float newHealth)
    {
        this._health = newHealth;
        this._healthBarTarget = new Vector3((newHealth / _maxHealth), HealthBar.localScale.y, HealthBar.localScale.z);
        HealthBar.localScale = _healthBarTarget;
    }

    [ClientRpc]
    private void RpcPlayerDazed()
    {
        if (!_dead)
        {
            this._movement.UpdatePlayerState(PlayerState.Dead);
        }
    }

    [ClientRpc]
    private void RpcPlayerUndazed()
    {
        if (!_dead)
        {
            this._movement.UpdatePlayerState(PlayerState.Alive);
        }
    }

    [ClientRpc]
    private void RpcPlayerDeath()
    {
      this._movement.UpdatePlayerState(PlayerState.Dead);
    }

    [ClientRpc]
    private void RpcPlayerRespawn()
    {
        this._movement.UpdatePlayerState(PlayerState.Respawn);
    }
    #endregion

    #region Commands

    [Command]
    private void CmdPlayerRespawn()
    {
        PlayerRespawnHook(true, false);
    }

    [Command]
    private void CmdPlayerUndazed()
    {
        _dazed = false;

        RpcPlayerUndazed();
    }

    #endregion

    #region Hook Functions

    public void PlayerRespawnHook(bool oldValue, bool newValue)
    {
        if (isServer)
        {
            this._dead = newValue;
            ModifyHealth(_maxHealth);
            RpcPlayerRespawn();
        }
    }

    public void PlayerUndazed()
    {
        if (hasAuthority && _dazed)
        {
            if ((Time.time - _dazedTimeStart) > _dazeDuration)
            {
                CmdPlayerUndazed();
            }
        }
    }

    #endregion
}
