using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using Steamworks;

public class PlayerObjectController : NetworkBehaviour
{
    #region player data
    [SyncVar] public int ConnectionId;
    [SyncVar] public int PlayerIdNumber;
    [SyncVar] public ulong PlayerSteamId;
    [SyncVar(hook = nameof(PlayerNameUpdate))] public string PlayerName;       // Sync variable with hook: every time var changes function is called
    [SyncVar(hook = nameof(PlayerReadyUpdate))] public bool Ready;
    #endregion

    private CustomNetworkManager manager;
    private CustomNetworkManager Manager
    {
        get
        {
            if(manager != null)
            {
                return manager;
            }
            return manager = CustomNetworkManager.singleton as CustomNetworkManager;
        }
    }

    #region Start Functions
    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public override void OnStartAuthority()
    {
        CmdSetPlayerName(SteamFriends.GetPersonaName().ToString());

        transform.tag = "LocalPlayer";
        transform.Find("PlayerObject").tag = "LocalCameraTarget";
        transform.Find("PuppetMaster").tag = "LocalRagdoll";

        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            LobbyController.Instance.FindLocalPlayer();
            LobbyController.Instance.UpdateLobbyName();
        }
    }

    public override void OnStartClient()
    {
        Manager.GamePlayers.Add(this);

        transform.Find("PlayerObject").Find("PlayerObjectModel").GetComponent<SkinnedMeshRenderer>().material = Manager.PlayerMaterials[PlayerIdNumber - 1];

        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            LobbyController.Instance.UpdateLobbyName();
            LobbyController.Instance.UpdatePlayerList();
        }
    }

    public override void OnStopClient()
    {
        Manager.GamePlayers.Remove(this);

        if(SceneManager.GetActiveScene().name == "Lobby")
            LobbyController.Instance.UpdatePlayerList();
    }
    #endregion

    #region Commands
    [Command]
    private void CmdSetPlayerName(string playerName)
    {
        this.PlayerNameUpdate(this.PlayerName, playerName);
    }

    [Command]
    private void CmdSetPlayerReady()
    {
        this.PlayerReadyUpdate(this.Ready, !this.Ready);
    }

    public void ChangeReady()
    {
        if (hasAuthority)   // If local player
            CmdSetPlayerReady();
    }

    [Command]
    public void CmdCanStartGame(string SceneName)
    {
        manager.StartGame(SceneName);
    }

    public void CanStartGame(string SceneName)
    {
        if (hasAuthority)   // If local player
            this.CmdCanStartGame(SceneName);
    }
    #endregion

    public void PlayerReadyUpdate(bool oldValue, bool newValue)
    {
        if (isServer)
        {
            this.Ready = newValue;
        }
        if (isClient)
        {
            LobbyController.Instance.UpdatePlayerList();
        }
    }

    public void PlayerNameUpdate(string oldValue, string newValue)
    {
        if (isServer)   // host
        {
            this.PlayerName = newValue;
        }
        if (isClient)   // client
        {
            LobbyController.Instance.UpdatePlayerList();
        }
    }




}
