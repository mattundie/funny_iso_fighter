using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Steamworks;

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

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.PageUp))
            CmdModifyHealth(Health + 5);
        if (Input.GetKeyDown(KeyCode.PageDown))
            CmdModifyHealth(Health - 5);
    }

    private void FixedUpdate()
    {
        if (Health < 20)
        {
            HealthBar.gameObject.GetComponent<Image>().color = Color.red;
            if (HealthBarIcon.sprite != HealthBarSad)
                HealthBarIcon.sprite = HealthBarSad;
        }
        else if (Health >= 20)
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
        if (newHealth >= 0 && newHealth <= MaxHealth)
            this.HealthChanged(this.Health, newHealth);
        else if (newHealth > MaxHealth)
            this.HealthChanged(this.Health, MaxHealth);
        else if (newHealth < 0)
            this.HealthChanged(this.Health, 0);
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
