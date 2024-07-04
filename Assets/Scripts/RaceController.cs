using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;

public class RaceController : NetworkBehaviour
{
    public static RaceController Instance;
    public int numPlayers;

    [SerializeField]
    private List<Player> _players = new List<Player>();
    private CircuitController _circuitController;
    private GameObject[] _debuggingSpheres;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        if(IsServer)GetComponent<NetworkObject>().Spawn();
    }

    private void Start()
    {
        if (_circuitController == null) _circuitController = GetComponent<CircuitController>();

        _debuggingSpheres = new GameObject[PlayersManager.Instance.PlayersInGame];
        for (int i = 0; i < PlayersManager.Instance.PlayersInGame; ++i)
        {
            _debuggingSpheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _debuggingSpheres[i].GetComponent<SphereCollider>().enabled = false;
        }

        // Obtener todos los jugadores al inicio
        foreach (var kvp in PlayersManager.Instance.GetSpawnedPlayers())
        {
            var playerObject = kvp.Value;
            var player = playerObject.GetComponent<Player>();
            if (player != null)
            {
                AddPlayer(player);
            }
        }
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        if (_players.Count == 0)
            return;

        UpdateRaceProgress();
    }

    public void AddPlayer(Player player)
    {
        _players.Add(player);
        numPlayers++;
    }

    public void RemovePlayer(Player player)
    {
        _players.Remove(player);
    }

    private class PlayerInfoComparer : Comparer<Player>
    {
        private readonly float[] _arcLengths;

        public PlayerInfoComparer(float[] arcLengths)
        {
            _arcLengths = arcLengths;
        }

        public override int Compare(Player x, Player y)
        {
            //Ya que el ownerclientID se inicia desde 1, es necesario restarle 1 cuando se quiere ordenar todos los elementos de la lista.
            if (_arcLengths[x.OwnerClientId] < _arcLengths[y.OwnerClientId])
            {
                Debug.Log(_arcLengths[x.OwnerClientId] + " > " + _arcLengths[y.OwnerClientId]);
                return 1;
            }
            else
            {
                Debug.Log(_arcLengths[x.OwnerClientId] + " < " + _arcLengths[y.OwnerClientId]);
                return -1;
            }
        }
    }

    public void UpdateRaceProgress()
    {

        if (_players.Count == 0)
            return;

        // Update car arc-lengths
        float[] arcLengths = new float[_players.Count];

        for (int i = 0; i < _players.Count; ++i)
        {
            if (_players[i] == null)
            {
                return;
            }
            try
            {
                arcLengths[i] = ComputeCarArcLength(i);
            }
            catch (IndexOutOfRangeException)
            {
                Debug.Log("Se ha eliminado ese jugador");
                return;
            }
        }

        _players.Sort(new PlayerInfoComparer(arcLengths)); // Se ordenan los jugadores

        string myRaceOrder = "";
        foreach (var player in _players)
        {
            myRaceOrder += player.Name + " ";
        }

        Debug.Log("Race order: " + myRaceOrder);
    }

    [ClientRpc]
    private void UpdateRaceOrderClientRpc(string raceOrder)
    {
        Debug.Log("Client Race order: " + raceOrder);
    }

    float ComputeCarArcLength(int id)
    {
        // Compute the projection of the car position to the closest circuit 
        // path segment and accumulate the arc-length along of the car along
        // the circuit.
        Vector3 carPos = this._players[id].car.transform.position;

        float minArcL =
            this._circuitController.ComputeClosestPointArcLength(carPos, out _, out var carProj, out _);

        this._debuggingSpheres[id].transform.position = carProj;

        if (this._players[id].CurrentLap.Value == 0)
        {
            minArcL -= _circuitController.CircuitLength;
        }
        else
        {
            minArcL += _circuitController.CircuitLength *
                       (_players[id].CurrentLap.Value - 1);
        }

        return minArcL;
    }
}
