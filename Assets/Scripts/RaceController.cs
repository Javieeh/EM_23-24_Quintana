using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public class RaceController : NetworkBehaviour
{
    public static RaceController Instance;
    public int numPlayers;

    public List<Player> _players = new(4);
    public List<CheckpointManager> carCheckManagerList; // Lista de todos los CheckpointManagers de los coches
    private CircuitController _circuitController;
    private GameObject[] _debuggingSpheres;
    
    private void Start()
    {
        if(IsServer) StartCoroutine(CheckAllPlayersReady());
        if (_circuitController == null) _circuitController = GetComponent<CircuitController>();
        _players = new GameManager().GetPlayers();
        _debuggingSpheres = new GameObject[GameManager.Instance.numPlayers];
        for (int i = 0; i < GameManager.Instance.numPlayers; ++i)
        {
            _debuggingSpheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _debuggingSpheres[i].GetComponent<SphereCollider>().enabled = false;
        }
        Debug.Log("Sigan viendo");
            var aux = FindObjectsOfType<CheckpointManager>();
            foreach (var item in aux){
                carCheckManagerList.Add(item);
                Debug.Log(item);
            }
            carCheckManagerList.Sort((a, b) =>
            {
                if (a.LapsCompleted.Value != b.LapsCompleted.Value)
                    return b.LapsCompleted.Value.CompareTo(a.LapsCompleted.Value);

                return b.CurrentCheckpoint.Value.CompareTo(a.CurrentCheckpoint.Value);
            });
    }

    private void Update()
    {
        /*if (_players.Count == 0)
            return;*/
        if (!IsServer)
        {
            
            Debug.Log(carCheckManagerList.Count);
            // Enviar posiciones a todos los clientes
            for (int i = 0; i < carCheckManagerList.Count; i++)
            {
                carCheckManagerList[i].GetComponentInChildren<CarController>().UpdatePositionClientRpc(i + 1);
            }

        UpdateRaceProgress();
        }
    }

    public void AddPlayer(Player player)
    {
        _players.Add(player);
    }
    public void RemovePlayer(Player player)
    {
        _players.Remove(player);
    }

    private class PlayerInfoComparer : Comparer<Player>
    {
        readonly float[] _arcLengths;

        public PlayerInfoComparer(float[] arcLengths)
        {
            _arcLengths = arcLengths;
        }

        public override int Compare(Player x, Player y)
        {
            if (_arcLengths[x.ID] < _arcLengths[y.ID])
                return 1;
            else return -1;
        }
    }

    public void UpdateRaceProgress()
    {
        // Update car arc-lengths
        float[] arcLengths = new float[_players.Count];

        for (int i = 0; i < _players.Count; ++i)
        {
            arcLengths[i] = ComputeCarArcLength(i);
        }

        _players.Sort(new PlayerInfoComparer(arcLengths));

        string myRaceOrder = "";
        foreach (var player in _players)
        {
            myRaceOrder += player.Name + " ";
        }
        for (int i = 0; i < _players.Count; i++){
            Player player = _players[i];
            myRaceOrder += player.ID + " ";
            //Actualizamos la UI
            UIManager.Instance.UpdatePlayerPosition(player.Name, i+1);
        }

        Debug.Log("Race order: " + myRaceOrder);
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

        if (this._players[id].CurrentLap == 0)
        {
            minArcL -= _circuitController.CircuitLength;
        }
        else
        {
            minArcL += _circuitController.CircuitLength *
                       (_players[id].CurrentLap - 1);
        }

        return minArcL;
    }
    // FUNCION QUE DA COMIENZO A LA CARRERA DE FORMA SINCRONIZADA
    private IEnumerator CheckAllPlayersReady(){
        List<CarController> carControllers = new List<CarController>();
        foreach(var player in _players){
            carControllers.Add(player.GetComponentInChildren<CarController>());
        }
        while (true)
        {
            
            if (carControllers.All(CarController => CarController.IsReady.Value))
            {
                StartRace(carControllers);
                yield break;
            }
            yield return new WaitForSeconds(.5f); // Esperar un segundo antes de volver a comprobar
        }        
    }
    private void StartRace(List<CarController> listCarContr)
    {
        foreach (var carContr in listCarContr)
        {
            carContr.StartRaceClientRpc();
        }
    }
    
}