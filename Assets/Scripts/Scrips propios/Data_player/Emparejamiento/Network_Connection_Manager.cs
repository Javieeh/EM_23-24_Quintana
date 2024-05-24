using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Network_Connection_Manager : NetworkBehaviour
{
    public int maxPlayers = 4;
    private List<ulong> playerIds = new List<ulong>();

    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
        }
        else
        {
            Debug.LogError("NetworkManager is not initialized. Make sure you have a NetworkManager in the scene.");
        }
    }
    public void StartHost()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.StartHost();
            Debug.Log("Host started");
        }
    }

    public void StartClient()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.StartClient();
            Debug.Log("Client started");
        }
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        Debug.Log("Client connected: " + clientId);
        playerIds.Add(clientId);

        if (playerIds.Count >= maxPlayers)
        {
            CreateNewLobby();
        }
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
        Debug.Log("Client disconnected: " + clientId);
        playerIds.Remove(clientId);
    }

    private void CreateNewLobby()
    {
        Debug.Log("Creating new lobby because the current one is full.");
        // Lógica para crear una nueva sala o cambiar la escena.
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        }
    }
}
// Start is called before the first frame update


  

