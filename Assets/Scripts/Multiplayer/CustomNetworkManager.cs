using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.SceneManagement;

public class CustomNetworkManager : NetworkManager
{
    [SerializeField] private PlayerObjectController GamePlayerPrefab;
    public Material[] PlayerMaterials;
    public Vector3[] SpawnPoints;
    public List<PlayerObjectController> GamePlayers { get; } = new List<PlayerObjectController>();

    // Called every time a player is added to the server
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if(SceneManager.GetActiveScene().name == "Lobby")
        {
            PlayerObjectController gamePlayerInstance = Instantiate(GamePlayerPrefab);
            gamePlayerInstance.ConnectionId = conn.connectionId;
            gamePlayerInstance.PlayerIdNumber = GamePlayers.Count + 1;
            gamePlayerInstance.PlayerSteamId = (ulong)SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)SteamLobby.Instance.CurrentLobbyID, GamePlayers.Count);

            NetworkServer.AddPlayerForConnection(conn, gamePlayerInstance.gameObject);
        }
    }

    public void StartGame(string SceneName)
    {
        ServerChangeScene(SceneName);
    }

    public void LeaveLobby()
    {
        SceneManager.LoadScene(0);
    }
}
