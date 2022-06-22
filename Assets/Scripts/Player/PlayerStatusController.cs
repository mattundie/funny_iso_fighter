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
    [SyncVar(hook = nameof(HealthChanged))] public float _health;
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
        if(Input.GetKeyDown(KeyCode.PageUp))
            CmdModifyHealth(_health + 5);
        if (Input.GetKeyDown(KeyCode.PageDown))
            CmdModifyHealth(_health - 5);

        if (Input.GetKeyDown(KeyCode.R))
            CmdPlayerRespawn();
    }

    private void FixedUpdate()
    {
        if (isClient)
        {
            PopulateHealthUI();
        }

        if (GetComponent<PlayerMovementNew>()._rb.transform.position.magnitude > 120f)
        {
            CmdPlayerDeath();
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


    #region Commands
    [Command]
    private void CmdModifyHealth(float newHealth)
    {
        if (newHealth >= 0 && newHealth <= _maxHealth)
            this.HealthChanged(this._health, newHealth);
        else if (newHealth > _maxHealth)
            this.HealthChanged(this._health, _maxHealth);
        else if (newHealth < 0)
            this.HealthChanged(this._health, 0);

        if (newHealth == 0)
            CmdPlayerDeath();
    }

    [Command]
    private void CmdPlayerDeath()
    {
        if(!_dead)
        {
            PlayerDeathEvent(false, true);
        }
    }

    [Command]
    private void CmdPlayerRespawn()
    {
        if (_dead)
        {
            PlayerRespawnEvent(true, false);
        }
    }
    #endregion

    #region Hook Functions
    private void HealthChanged(float oldValue, float newValue)
    {
        if (isServer)
        {
            this._health = newValue;
        }
        if (isClient)
        {
            _healthBarTarget = new Vector3((newValue / _maxHealth), HealthBar.localScale.y, HealthBar.localScale.z);
            HealthBar.localScale = _healthBarTarget;
        }
    }

    private void PlayerDeathEvent(bool oldValue, bool newValue)
    {
        if (isServer)
        {
            this._dead = newValue;
            this._deaths += 1;
            GetComponent<PlayerMovementNew>().UpdatePlayerState(PlayerState.Dead);
        }
    }
    private void PlayerRespawnEvent(bool oldValue, bool newValue)
    {
        if (isServer)
        {
            this._health = this._maxHealth;
            this._dead = newValue;
            GetComponent<PlayerMovementNew>().UpdatePlayerState(PlayerState.Respawn);
        }
    }
    #endregion
}
