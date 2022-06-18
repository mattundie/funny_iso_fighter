using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;

public class PlayerStatusController : NetworkBehaviour
{
    #region Components
    [Header("Components")]
    public Transform HealthBar;
    #endregion

    #region Status Data
    [Header("Status Data")]
    [SyncVar(hook = nameof(HealthChanged))] public float Health;
    [SyncVar] public float MaxHealth = 100;
    #endregion

    #region Settings
    [Header("Settings")]
    
    #endregion

    private Vector3 _healthBarTarget;

    private void Start()
    {
        Health = MaxHealth;
    }

    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.DownArrow))
            if (hasAuthority)
                this.CmdModifyHealth(Health - 0.1f);
    }


    #region Commands
    [Command]
    private void CmdModifyHealth(float newHealth)
    {
        if(Health >= 0 && Health < MaxHealth)
            this.HealthChanged(this.Health, newHealth);
    }
    #endregion

    #region Hook Functions
    private void HealthChanged(float oldValue, float newValue)
    {
        if (isServer)
            this.Health = newValue;
        if (isClient)
        {
            _healthBarTarget = new Vector3((newValue / MaxHealth), HealthBar.localScale.y, HealthBar.localScale.z);
            HealthBar.localScale = _healthBarTarget;
        }
    }
    #endregion
}
