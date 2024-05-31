using System.Globalization;
using UnityEngine;
using UnityEngine.Serialization;

public class Player : MonoBehaviour
{
    // Player Info
    public string Name { get; set; }
    public int ID { get; set; }

    // Race Info
    public GameObject car;
    public Color CarColor { get; set; }
    public int CurrentPosition { get; set; }
    public int CurrentLap { get; set; }

    //Canvas



    //vida del coche
    public int life;

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
        life = 5;
    }

    private void Update()
    {
        if (life <= 0) // en caso de quedarse sin vida, destruimos
        {
            Debug.Log("jugador eliminado");

            //destruimos
            Destroy(gameObject);
        }
    }
}