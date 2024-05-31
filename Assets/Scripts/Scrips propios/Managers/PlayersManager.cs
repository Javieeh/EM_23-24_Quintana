using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayersManager : Singleton<PlayersManager>
{
    [SerializeField]
    private Transform[] placeholders; // Array de posiciones para los jugadores en el lobby

    private NetworkVariable<int> playersInGame = new NetworkVariable<int>();
    private int nextPlaceholderIndex = 0; // �ndice para el siguiente placeholder disponible
    private int readyPlayersCount = 0; // Conteo de jugadores listos
    private Dictionary<ulong, GameObject> spawnedPlayers = new Dictionary<ulong, GameObject>();

    [SerializeField] private GameObject prefab;
    [SerializeField] private int countdownTime = 3; // Tiempo de cuenta atr�s en segundos

    public int PlayersInGame
    {
        get
        {
            return playersInGame.Value;
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
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
            if (!spawnedPlayers.ContainsKey(clientId))
            {
                SpawnPlayer(clientId, "Player" + clientId); // Asignar un nombre inicial basado en el ID del cliente
            }
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (IsServer)
        {
            Debug.Log("Someone disconnected...");
            playersInGame.Value--;
            // Aqu� podr�as implementar la l�gica para liberar el placeholder si es necesario
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

        // Obt�n el prefab del jugador registrado en el NetworkManager
        GameObject playerPrefab = prefab;

        if (playerPrefab == null)
        {
            Debug.LogError("Player Prefab is not set in the NetworkManager.");
            return;
        }

        // Instancia el jugador en la posici�n del placeholder y con la rotaci�n predeterminada
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

        // Evitar que el jugador se destruya al cargar una nueva escena
        DontDestroyOnLoad(player);

        // A�adir el jugador al diccionario para evitar re-instantanciaci�n
        spawnedPlayers.Add(clientId, player);

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
        LoadCircuitSceneClientRpc();
    }

    [ClientRpc]
    private void LoadCircuitSceneClientRpc()
    {
        StartCoroutine(LoadCircuitScene());
    }

    private IEnumerator LoadCircuitScene()
    {
        // Cargar la escena del circuito
        SceneManager.LoadScene("Nascar");

        // Esperar a que la escena se cargue
        yield return new WaitForSeconds(1f);

        Debug.Log("Loaded CircuitScene, moving players...");

        // Obtener las posiciones de inicio en la escena del circuito
        Transform[] startPositions = GetStartPositions();

        // Mover a los jugadores a las posiciones iniciales en la nueva escena
        int index = 0;
        foreach (var player in spawnedPlayers.Values)
        {
            if (index < startPositions.Length)
            {
                Debug.Log($"Moving player {player.name} to position {startPositions[index].position}");
                player.transform.position = startPositions[index].position;
                player.transform.rotation = startPositions[index].rotation;
                index++;
            }
            else
            {
                Debug.LogError("Not enough start positions for players.");
                break;
            }
        }
    }

    public Transform GetStartPosition(ulong clientId)
    {
        int index = 0;
        foreach (var kvp in spawnedPlayers)
        {
            if (kvp.Key == clientId)
            {
                return GetStartPositions()[index];
            }
            index++;
        }
        return null;
    }

    private Transform[] GetStartPositions()
    {
        // Encuentra los objetos de posici�n de inicio en la escena del circuito
        GameObject[] startObjects = GameObject.FindGameObjectsWithTag("StartPosition");
        Transform[] startPositions = new Transform[startObjects.Length];
        for (int i = 0; i < startObjects.Length; i++)
        {
            startPositions[i] = startObjects[i].transform;
        }
        return startPositions;
    }

    private void Update()
    {
        // Opcional: cualquier l�gica de actualizaci�n
    }
}

public class Singleton<T> : NetworkBehaviour where T : Component
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
                    _instance = objs[0];
                }
                if (objs.Length > 1)
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
