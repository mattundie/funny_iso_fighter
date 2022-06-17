using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using Steamworks;

public class SteamLobby : MonoBehaviour
{
    public static SteamLobby Instance;
    #region Callbacks
    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> JoinRequested;
    protected Callback<LobbyEnter_t> LobbyEntered;
    #endregion

    public ulong CurrentLobbyID;

    private const string HostAddressKey = "HostAddress";
    private CustomNetworkManager manager;

    private void Start()
    {
        if(!SteamManager.Initialized) { return; };     // If steam is not open, return

        if(Instance == null) { Instance = this; }

        manager = GetComponent<CustomNetworkManager>();

        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequested);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void HostLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, manager.maxConnections);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if(callback.m_eResult != EResult.k_EResultOK) { return;  }

        Debug.Log("Lobby Created");

        // Start hosting the game
        manager.StartHost();

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", $"{SteamFriends.GetPersonaName()}'s Lobby");
    }

    private void OnJoinRequested(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("Request To Join Lobby");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    // Called for both host and client
    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        // Both Host & Client Code
        CurrentLobbyID = callback.m_ulSteamIDLobby;

        if(NetworkServer.active) { return; };   // return if you are the host

        // Client Specific Code
        manager.networkAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);
        manager.StartClient();
    }

}
