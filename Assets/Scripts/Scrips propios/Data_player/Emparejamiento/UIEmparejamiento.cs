using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine;

public class UIEmparejamiento : MonoBehaviour
{
    public Text playersListText;

    private List<string> playerNames = new List<string>();

    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        string playerName = $"Player {clientId}";
        playerNames.Add(playerName);
        UpdatePlayersList();
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
        string playerName = $"Player {clientId}";
        playerNames.Remove(playerName);
        UpdatePlayersList();
    }

    private void UpdatePlayersList()
    {
        playersListText.text = "Players:\n";
        foreach (string name in playerNames)
        {
            playersListText.text += name + "\n";
        }
    }

    private void OnDestroy()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
    }
}
