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
    [SyncVar] public int _deaths;
    #endregion

    #region Settings
    
    #endregion

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
        if (_health == 0 && !_dead)
            PlayerDeath();

        if (!hasAuthority) { return; }

        if (Input.GetKeyDown(KeyCode.R) && _dead)
            CmdPlayerRespawn();
    }

    private void FixedUpdate()
    {
        if (isClient)
        {
            PopulateHealthUI();
        }

        if(!hasAuthority) { return; }

        if (GetComponent<PlayerMovementNew>()._rb.transform.position.magnitude > 100f)
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

    private void ModifyHealth(float modifier)
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

    private void PlayerDeath()
    {
        if (!isServer) { return; }

        _dead = true;
        _deaths += 1;

        RpcPlayerDeath();
    }

    #region ClientRpc Calls

    [ClientRpc]
    private void RpcHealthChanged(float newHealth)
    {
        this._health = newHealth;
        _healthBarTarget = new Vector3((newHealth / _maxHealth), HealthBar.localScale.y, HealthBar.localScale.z);
        HealthBar.localScale = _healthBarTarget;
    }

    [ClientRpc]
    private void RpcPlayerDeath()
    {
      this.GetComponent<PlayerMovementNew>().UpdatePlayerState(PlayerState.Dead);
    }

    [ClientRpc]
    private void RpcPlayerRespawn()
    {
        this.GetComponent<PlayerMovementNew>().UpdatePlayerState(PlayerState.Respawn);
    }
    #endregion

    #region Commands

    [Command]
    private void CmdPlayerRespawn()
    {
        PlayerRespawnHook(true, false);
    }

    #endregion

    #region Hook Functions

    public void PlayerRespawnHook(bool oldValue, bool newValue)
    {
        if (isServer)
        {
            this._dead = newValue;
            this._health = this._maxHealth;
            RpcPlayerRespawn();
        }
    }

    #endregion
}
