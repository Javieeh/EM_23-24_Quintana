using System.Collections;
using System.ComponentModel;
using System.Globalization;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

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

    //
    public int numeroMuertes;
    public int points;
    public bool isDie;

    //Materiales
    private Renderer originRender;
    private Material originMaterial;

    //Id
    public int id;

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
        numeroMuertes = 0;
        points = 0;
        isDie = false;
        id = 0;

        originRender = GetComponentInChildren<Renderer>();
        originMaterial = originRender.material;
    }


    private void Update()
    {
        if (life <= 0 && isDie == false) // en caso de quedarse sin vida, destruimos
        {
            Debug.Log("jugador eliminado");
            numeroMuertes++;
            isDieDie();
        }
    }

    IEnumerator CountDown()
    {
        Debug.Log("cooldown...");
        this.life = 5;
        isDie = false;
        yield return new WaitForSeconds(5);
        //volvermos a establecer el tag
        
        this.transform.GetChild(0).transform.tag = "Player";
    }


    public void isDieDie()
    {
        isDie = true;

        //Quitamos tag para que no se pueda disparar mas
        this.transform.GetChild(0).transform.tag = "Untagged";

        //reajustamos material
        originRender.material = originMaterial;

        Debug.Log("Empezando cooldown");
        StartCoroutine(CountDown());
    }

}