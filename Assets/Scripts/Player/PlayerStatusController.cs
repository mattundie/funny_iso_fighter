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
    public Image HealthBarIcon;
    public Sprite HealthBarHappy;
    public Sprite HealthBarSad;
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
    }

    private void Update()
    {
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

        if (GetComponent<PlayerMovementController>()._rb.transform.position.magnitude > 100f)
        {
            PlayerDeath();
            CmdPlayerRespawn();
        }
    }

    [Client]
    private void PopulateHealthUI()
    {
        if (_health < 20)
        {
            HealthBar.gameObject.GetComponent<Image>().color = Color.red;
            if (HealthBarIcon.sprite != HealthBarSad)
                HealthBarIcon.sprite = HealthBarSad;
        }
        else if (_health >= 20)
        {
            HealthBar.gameObject.GetComponent<Image>().color = Color.green;
            if (HealthBarIcon.sprite != HealthBarHappy)
                HealthBarIcon.sprite = HealthBarHappy;
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
            this.GetComponent<PlayerMovementController>().UpdatePlayerState(PlayerState.Dead);
        }
    }

    [ClientRpc]
    private void RpcPlayerUndazed()
    {
        if (!_dead)
        {
            this.GetComponent<PlayerMovementController>().UpdatePlayerState(PlayerState.Alive);
        }
    }

    [ClientRpc]
    private void RpcPlayerDeath()
    {
      this.GetComponent<PlayerMovementController>().UpdatePlayerState(PlayerState.Dead);
    }

    [ClientRpc]
    private void RpcPlayerRespawn()
    {
        this.GetComponent<PlayerMovementController>().UpdatePlayerState(PlayerState.Respawn);
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
