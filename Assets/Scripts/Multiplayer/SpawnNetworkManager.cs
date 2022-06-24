using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnNetworkManager : MonoBehaviour
{
    public GameObject _networkManagerPrefab;
    public Button _hostButton;
    [SerializeField] private GameObject _networkManager;

    private void Awake()
    {
        _networkManager = GameObject.FindGameObjectWithTag("NetworkManager");

        if (!_networkManager)
            _networkManager = Instantiate(_networkManagerPrefab, Vector3.zero, Quaternion.identity);

        _hostButton.onClick.AddListener(() => _networkManager.GetComponent<SteamLobby>().HostLobby());
    }
}
