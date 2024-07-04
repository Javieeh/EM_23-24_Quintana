using System.Collections;
using System.ComponentModel;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    // Player Info
    public string Name { get; set; }
    public int ID { get; set; }

    // Race Info
    public GameObject car;
    public Color CarColor { get; set; }
    public int CurrentPosition { get; set; }
    public int CurrentLap { get; set; }

    //
    public int numeroMuertes;
    public int points;
    public bool isDie;

    //Id
    public int id;

    //vida del coche
    public NetworkVariable<int> life = new NetworkVariable<int>(5);
    private Coroutine cooldownCoroutine;


    //public override void OnNetworkSpawn()
    //{
    //    if (IsOwner)
    //    {
    //        // Asignar color y nombre del jugador
    //        CarColor = // color seleccionado
    //        Name = // nombre ingresado
    //    }

    //    playerColor.OnValueChanged += OnColorChanged;
    //    playerName.OnValueChanged += OnNameChanged;

    //    OnColorChanged(playerColor.Value, playerColor.Value);
    //    OnNameChanged(playerName.Value, playerName.Value);
    //}

    public override string ToString()
    {
        return Name;
    }

    private void Start()
    {
        //GameManager.Instance.currentRace.AddPlayer(this);
        numeroMuertes = 0;
        points = 0;
        isDie = false;
    }

    private IEnumerator CooldownCoroutine()
    {
        Debug.Log("Jugador eliminado. Iniciando cooldown de 10 segundos.");
        SetPlayerTag(false);

        yield return new WaitForSeconds(10);

        life.Value = 5; // Reiniciamos la vida para simplificar el ejemplo
        SetPlayerTag(true);
        cooldownCoroutine = null;

        Debug.Log("Cooldown finalizado. Jugador reactivado.");
    }

    private void SetPlayerTag(bool isActive)
    {
        // Asignar o quitar la etiqueta "Player"
        var playerTag = isActive ? "Player" : "Untagged";
        transform.GetChild(0).tag = playerTag;
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        if (!IsServer)
            return;

        life.Value -= damage;
        Debug.Log($"Vida del jugador después del daño: {life.Value}");

        if (life.Value <= 0)
        {
            // Aquí puedes manejar la destrucción del coche o el reset de vida
            Debug.Log("Jugador eliminado");
            life.Value = 5; // Reiniciamos la vida para simplificar el ejemplo
        }
    }
}