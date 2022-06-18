using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class LobbyController : MonoBehaviour
{
    public static LobbyController Instance;


    #region UI Elements
    public TMP_Text LobbyNameText;

    #endregion

    #region Player Data
    public GameObject PlayerListViewContent;
    public GameObject PlayerListItemPrefab;
    public GameObject LocalPlayerObject;
    #endregion

    #region Misc Data
    public ulong CurrentLobbyId;
    public bool PlayerItemCreated = false;
    private List<PlayerListItem> PlayerListItems = new List<PlayerListItem>();
    public PlayerObjectController LocalPlayerController;
    #endregion

    #region Ready Data
    public Button StartGameButton;
    public TMP_Text ReadyButtonText;
    #endregion

    #region Manager
    private CustomNetworkManager manager;
    private CustomNetworkManager Manager
    {
        get
        {
            if (manager != null)
            {
                return manager;
            }
            return manager = CustomNetworkManager.singleton as CustomNetworkManager;
        }
    }
    #endregion

    private void Awake()
    {
        if(Instance == null) { Instance = this; }
    }

    public void ReadyPlayer()
    {
        LocalPlayerController.ChangeReady();
    }

    public void UpdateButton()
    {
        if(LocalPlayerController.Ready == true)
        {
            ReadyButtonText.text = "Cancel";
            ReadyButtonText.transform.parent.GetComponent<Image>().color = new Color(195, 113, 113);
        }
        else
        {
            ReadyButtonText.text = "Ready";
            ReadyButtonText.transform.parent.GetComponent<Image>().color = new Color(49, 190, 165);
        }
    }

    public void CheckIfAllReady()
    {
        bool allReady = false;

        foreach(PlayerObjectController player in Manager.GamePlayers)
        {
            if (player.Ready)
            {
                allReady = true;
            }
            else
            {
                allReady = false;
                break;
            }
        }

        if (allReady)
        {
            if (LocalPlayerController.PlayerIdNumber == 1)  // If the host
            {
                StartGameButton.interactable = true;
            }
            else
            {
                StartGameButton.interactable = false;
            }
        }
        else
        {
            StartGameButton.interactable = false;
        }
    }

    public void UpdateLobbyName()
    {
        CurrentLobbyId = Manager.GetComponent<SteamLobby>().CurrentLobbyID;
        LobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyId), "name");
    }

    public void UpdatePlayerList()
    {
        if (!PlayerItemCreated) { CreateHostPlayerItem(); }

        if (PlayerListItems.Count < Manager.GamePlayers.Count) { CreateClientPlayerItem(); }

        if (PlayerListItems.Count > Manager.GamePlayers.Count) { RemovePlayerItem(); }

        if (PlayerListItems.Count == Manager.GamePlayers.Count) { UpdatePlayerItem(); }
    }

    public void FindLocalPlayer()
    {
        LocalPlayerObject = GameObject.FindGameObjectWithTag("LocalPlayer");
        LocalPlayerController = LocalPlayerObject.GetComponent<PlayerObjectController>();
    }

    public void CreateHostPlayerItem()
    {
        foreach(PlayerObjectController player in Manager.GamePlayers)
        {
            GameObject newPlayerItem = Instantiate(PlayerListItemPrefab) as GameObject;
            PlayerListItem NewPlayerItemScript = newPlayerItem.GetComponent<PlayerListItem>();

            NewPlayerItemScript._playerName = player.PlayerName;
            NewPlayerItemScript._connectionId = player.ConnectionId;
            NewPlayerItemScript._playerSteamId = player.PlayerSteamId;
            NewPlayerItemScript._ready = player.Ready;
            NewPlayerItemScript.PopulateUI();

            newPlayerItem.transform.SetParent(PlayerListViewContent.transform);
            newPlayerItem.transform.localScale = Vector3.one;

            PlayerListItems.Add(NewPlayerItemScript);
        }

        PlayerItemCreated = true;
    }

    public void CreateClientPlayerItem()
    {
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            if(!PlayerListItems.Any(b => b._connectionId == player.ConnectionId))
            {
                GameObject newPlayerItem = Instantiate(PlayerListItemPrefab) as GameObject;
                PlayerListItem NewPlayerItemScript = newPlayerItem.GetComponent<PlayerListItem>();

                NewPlayerItemScript._playerName = player.PlayerName;
                NewPlayerItemScript._connectionId = player.ConnectionId;
                NewPlayerItemScript._playerSteamId = player.PlayerSteamId;
                NewPlayerItemScript._ready = player.Ready;
                NewPlayerItemScript.PopulateUI();

                newPlayerItem.transform.SetParent(PlayerListViewContent.transform);
                newPlayerItem.transform.localScale = Vector3.one;

                PlayerListItems.Add(NewPlayerItemScript);
            }
        }

    }

    public void UpdatePlayerItem()
    {
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            foreach(PlayerListItem PlayerListItemScript in PlayerListItems)
            {
                if(PlayerListItemScript._connectionId == player.ConnectionId)
                {
                    PlayerListItemScript._playerName = player.PlayerName;
                    PlayerListItemScript._ready = player.Ready;
                    PlayerListItemScript.PopulateUI();

                    if(player == LocalPlayerController) // update button for local player
                    {
                        UpdateButton();
                    }
                }
            }
        }

        CheckIfAllReady();
    }

    public void RemovePlayerItem()
    {
        List<PlayerListItem> playerListItemToRemove = new List<PlayerListItem>();

        foreach(PlayerListItem playerListItem in PlayerListItems)
        {
            if(!Manager.GamePlayers.Any(b => b.ConnectionId == playerListItem._connectionId))
            {
                playerListItemToRemove.Add(playerListItem);
            }
        }
        if(playerListItemToRemove.Count > 0)
        {
            foreach(PlayerListItem remove in playerListItemToRemove)
            {
                GameObject objToRemove = remove.gameObject;
                PlayerListItems.Remove(remove);
                Destroy(objToRemove);
                objToRemove = null;
            }
        }
    }
}
