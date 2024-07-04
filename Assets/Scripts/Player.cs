using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    // Player Info
    public NetworkVariable<NetworkString> Name = new NetworkVariable<NetworkString>();
    public NetworkVariable<int> ID = new NetworkVariable<int>();

    // Race Info
    public GameObject car;
    public NetworkVariable<Color> CarColor = new NetworkVariable<Color>();
    public NetworkVariable<int> CurrentPosition = new NetworkVariable<int>();
    public NetworkVariable<int> CurrentLap = new NetworkVariable<int>();

    // Otros atributos
    public int numeroMuertes;
    public int points;
    public bool isDie;

    // Materiales
    private Renderer originRender;
    private Material originMaterial;

    // Vida del coche
    public int life;

    // Referencias a PlayerName y PlayerColor
    private PlayerName playerName;
    private PlayerColor playerColor;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Aquí puedes inicializar los valores del jugador
            playerName = GetComponent<PlayerName>();
            playerColor = GetComponent<PlayerColor>();
            
            if (playerName != null && playerColor != null)
            {
                playerName.SetName("Player" + NetworkManager.Singleton.LocalClientId);
            }
        }

        // Suscribirse a los cambios
        Name.OnValueChanged += OnNameChanged;
        CarColor.OnValueChanged += OnColorChanged;
    }

    private void OnDestroy()
    {
        // Desuscribirse de los cambios
        Name.OnValueChanged -= OnNameChanged;
        CarColor.OnValueChanged -= OnColorChanged;
    }

    private void OnNameChanged(NetworkString oldName, NetworkString newName)
    {
        // Código para manejar cambios en el nombre del jugador
        this.Name.Value = newName;
        this.gameObject.name = newName.ToString();
        if (playerName != null)
        {
            playerName.SetName(newName.ToString());
        }
    }

    private void OnColorChanged(Color oldColor, Color newColor)
    {
        // Código para manejar cambios en el color del coche
        this.CarColor.Value = newColor;
        if (playerColor != null)
        {
            playerColor.ChangeColorServerRpc(newColor);
        }
    }

    private void Start()
    {
        life = 5;
        numeroMuertes = 0;
        points = 0;
        isDie = false;

        originRender = GetComponentInChildren<Renderer>();
        originMaterial = originRender.material;
    }

    private void Update()
    {
        if (life <= 0 && !isDie)
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
        this.transform.GetChild(0).transform.tag = "Player";
    }

    public void isDieDie()
    {
        isDie = true;
        this.transform.GetChild(0).transform.tag = "Untagged";
        originRender.material = originMaterial;

        Debug.Log("Empezando cooldown");
        StartCoroutine(CountDown());
    }

    [ServerRpc]
    public void UpdatePlayerAttributesServerRpc(ServerRpcParams rpcParams = default)
    {
        Name.Value = playerName.playerName.Value; ;
        CarColor.Value = playerColor.playerColor.Value; 
        ID.Value = (int)GetComponent<NetworkObject>().OwnerClientId;
        ActualizarAtributosClientRpc();
    }

    [ClientRpc]
    private void ActualizarAtributosClientRpc()
    {
        Name.Value = playerName.playerName.Value;
        CarColor.Value = playerColor.playerColor.Value;
        ID.Value = (int) GetComponent<NetworkObject>().OwnerClientId;

        this.gameObject.name = Name.Value;
        UpdateCarColor(CarColor.Value);
    }

    private void UpdateCarColor(Color color)
    {
        if (originRender != null)
        {
            originRender.material.color = color;
        }
    }
}
