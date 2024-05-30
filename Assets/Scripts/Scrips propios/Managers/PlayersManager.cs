using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayersManager : Singleton<PlayersManager>
{
    [SerializeField]
    private Transform[] placeholders; // Array de posiciones para los jugadores

    private NetworkVariable<int> playersInGame = new NetworkVariable<int>();
    private int nextPlaceholderIndex = 0; // Índice para el siguiente placeholder disponible
    private int readyPlayersCount = 0; // Conteo de jugadores listos

    [SerializeField] private GameObject prefab;
    [SerializeField] private int countdownTime = 3; // Tiempo de cuenta atrás en segundos

    public int PlayersInGame
    {
        get
        {
            return playersInGame.Value;
        }
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsServer)
        {
            Debug.Log("Someone connected...");
            playersInGame.Value++;
            SpawnPlayer(clientId, "Player" + clientId); // Asignar un nombre inicial basado en el ID del cliente
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (IsServer)
        {
            Debug.Log("Someone disconnected...");
            playersInGame.Value--;
            // Aquí podrías implementar la lógica para liberar el placeholder si es necesario
        }
    }

    public void SpawnPlayer(ulong clientId, string playerName)
    {
        if (nextPlaceholderIndex >= placeholders.Length)
        {
            Debug.LogError("Not enough placeholders for players.");
            return;
        }

        Transform spawnPoint = placeholders[nextPlaceholderIndex];
        Vector3 spawnPosition = spawnPoint.position;

        // Obtén el prefab del jugador registrado en el NetworkManager
        GameObject playerPrefab = prefab;

        if (playerPrefab == null)
        {
            Debug.LogError("Player Prefab is not set in the NetworkManager.");
            return;
        }

        // Instancia el jugador en la posición del placeholder y con la rotación predeterminada
        GameObject player = Instantiate(playerPrefab, spawnPosition, spawnPoint.rotation);
        NetworkObject networkObject = player.GetComponent<NetworkObject>();

        if (networkObject == null)
        {
            Debug.LogError("Player Prefab does not have a NetworkObject component.");
            Destroy(player);
            return;
        }

        // Marca el objeto como perteneciente al jugador local y lo instancia en la red
        networkObject.SpawnAsPlayerObject(clientId, true);

        // Asigna el nombre al jugador
        var playerNameComponent = player.GetComponent<PlayerName>();
        if (playerNameComponent != null)
        {
            playerNameComponent.SetName(playerName);
        }

        nextPlaceholderIndex++;
    }

    public void CheckReadyStatus()
    {
        readyPlayersCount = 0;

        var players = FindObjectsOfType<PlayerReady>();
        foreach (var player in players)
        {
            if (player.IsReady())
            {
                readyPlayersCount++;
            }
        }

        if (readyPlayersCount >= 2)
        {
            StartCoroutine(StartCountdown());
        }
    }

    private IEnumerator StartCountdown()
    {
        for (int i = countdownTime; i > 0; i--)
        {
            // Enviar el tiempo restante a los clientes
            UpdateCountdownClientRpc(i);
            yield return new WaitForSeconds(1f);
        }

        StartGame();
    }

    [ClientRpc]
    private void UpdateCountdownClientRpc(int timeRemaining)
    {
        //UIManager_gameObject.GetComponent<UIManager>().UpdateCountdownText(timeRemaining);
        UIManager.Instance.UpdateCountdownText(timeRemaining);
    }

    private void StartGame()
    {
        Debug.Log("Starting game...");
        // Aquí puedes añadir la lógica para iniciar la partida
    }

    private void Update()
    {
        // Opcional: cualquier lógica de actualización
    }
}



public class Singleton<T> : NetworkBehaviour
    where T : Component
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                var objs = FindObjectsOfType(typeof(T)) as T[];
                if (objs.Length > 0)
                {
                    _instance= objs[0];
                }
                if (objs.Length>1)
                {
                    Debug.LogError("There is more than one " + typeof(T).Name + " in the scene.");
                }
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = string.Format("_{0}", typeof(T).Name);
                    _instance = obj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }
}