using System.Collections;
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
    public bool isDie = false;
    public bool destroyPlayer = false;

    //vida del coche
    public int life;

    //Material
    private Material originMaterial;
    private Renderer originRender;

    //id
    public int id = 0;

    //puntos
    public int points;
    public int numeroMuertes;

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
        id = 0;
        
        //render
        originRender = this.GetComponentInChildren<Renderer>();
        originMaterial = originRender.material;

        //
        points = 0;
        numeroMuertes = 0;
        isDie = false;
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

    public void isDieDie()
    {
        isDie = true;

        //Quitamos tag para que no se pueda disparar mas
        this.transform.tag = "Untagged";

        //reajustamos material
        originRender.material = originMaterial;


        StartCoroutine(CountDown());
    }

    IEnumerator CountDown()
    {
        this.life = 5;
        isDie = false;
        yield return new WaitForSeconds(5);
        //volvermos a establecer el tag
        this.transform.tag = "Player";
        
    }
}