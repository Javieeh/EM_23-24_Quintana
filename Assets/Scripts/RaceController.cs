using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;

public class RaceController : NetworkBehaviour
{
    public static RaceController Instance;
    public int numPlayers;

    private NetworkVariable<int> countdown = new NetworkVariable<int>(0);

    [SerializeField]
    private List<Player> _players = new List<Player>();
    List<CarController> listCarContr;
    // salida sincr
    public NetworkVariable<bool> raceStarted = new NetworkVariable<bool>(false); //para comprobar si la partida se ha iniciado
    public NetworkVariable<float> remainingTime = new NetworkVariable<float>(5); //tiempo restante que queda para la salida
    private CircuitController _circuitController;
    private GameObject[] _debuggingSpheres;
    public GameObject countDownNUM;
    public GameObject startUI_GO;
    

    public TextMeshProUGUI textCountdown;
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
        if (IsServer) GetComponent<NetworkObject>().Spawn();
    }

    private void Start()
    {
        this.gameObject.GetComponent<NetworkObject>().Spawn();
        //if (IsServer) StartCoroutine(CheckAllPlayersReady());
        foreach (Player player in _players){
            player.GetComponentInChildren<Rigidbody>().isKinematic = true;
        }
        if (_circuitController == null) _circuitController = GetComponent<CircuitController>();

        _debuggingSpheres = new GameObject[PlayersManager.Instance.PlayersInGame];
        for (int i = 0; i < PlayersManager.Instance.PlayersInGame; ++i)
        {
            _debuggingSpheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _debuggingSpheres[i].GetComponent<SphereCollider>().enabled = false;
            _debuggingSpheres[i].GetComponent<MeshRenderer>().enabled = false;
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
        startUI_GO = GameObject.Find("StartGameUI");
        countDownNUM = GameObject.Find("Dynamic");
        textCountdown = countDownNUM.GetComponent<TextMeshProUGUI>();
        textCountdown.text = "";
    }

    private void Update()
    {
        UpdateRemainingTime();
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

        List<KeyValuePair<int, float>> sortedPlayers = new List<KeyValuePair<int, float>>();
        for (int i = 0; i < _players.Count; i++)
        {
            sortedPlayers.Add(new KeyValuePair<int, float>(i, arcLengths[i]));
        }

        sortedPlayers.Sort((x, y) => y.Value.CompareTo(x.Value));

        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            var playerIndex = sortedPlayers[i].Key;
            _players[playerIndex].CurrentPosition.Value = i + 1; // Posición basada en 1
        }

        // Notifica a todos los clientes para actualizar la interfaz
        //UpdatePlayerPositions();
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

    // FUNCION QUE DA COMIENZO A LA CARRERA DE FORMA SINCRONIZADA
    /*private IEnumerator CheckAllPlayersReady()
    {
        listCarContr = new List<CarController>();
        foreach (var player in _players)
        {
            listCarContr.Add(player.GetComponentInChildren<CarController>());
        }
        while (true)
        {

            if (listCarContr.All(CarController => CarController.IsReady.Value))
            {
                StartCoroutine(StartCountdown(listCarContr));
                yield break;
            }
            yield return new WaitForSeconds(.5f); // Esperar un segundo antes de volver a comprobar
        }
    }*/
    
    private void StartRace(List<CarController> listCarCMethod)
    {
        foreach (var carContr in listCarCMethod)
        {
            carContr.StartRaceClientRpc();
        }
    }
    
    public void UpdateRemainingTime()
    {
        
        //si el tiempo restante es 0 entonces la carrera empieza
        if (remainingTime.Value == 0)
        {
            Debug.Log("EMPIEZA LA CARRERA");            
            startUI_GO.SetActive(false);
            //Se le activa el input al jugador una vez haya terminado el tiempo de espera para que se pueda comenzar a mover
            ActivateInput();
        }
        else
        {
            Debug.Log("AUN NO EMPIEZA....");
            remainingTime.Value -= Time.deltaTime; //se va restando el tiempo al contador de tiempo restante
            textCountdown.text = "";
            if (remainingTime.Value < 0) //si el tiempo restante es menos que 0
            {
                remainingTime.Value = 0; //se asigna que sea directamente 0 para que sea más sencillo realizar comprobaciones
                textCountdown.text = "";
            }
            //Debug.Log(remainingTime.Value);
        }

    }
    public void ActivateInput()
    {
        foreach(Player player in _players){
            var rigido = player.GetComponentInChildren<Rigidbody>();
            rigido.isKinematic = false;
        }
    }

}
