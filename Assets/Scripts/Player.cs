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
    public NetworkVariable<int> life = new NetworkVariable<int>(5);

    private Coroutine cooldownCoroutine;

    // Referencias a PlayerName y PlayerColor
    private PlayerName playerName;
    private PlayerColor playerColor;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Aqu� puedes inicializar los valores del jugador
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
        // C�digo para manejar cambios en el nombre del jugador
        this.Name.Value = newName;
        this.gameObject.name = newName.ToString();
        if (playerName != null)
        {
            playerName.SetName(newName.ToString());
        }
    }

    private void OnColorChanged(Color oldColor, Color newColor)
    {
        // C�digo para manejar cambios en el color del coche
        this.CarColor.Value = newColor;
        if (playerColor != null)
        {
            playerColor.ChangeColorServerRpc(newColor);
        }
    }

    private void Start()
    {
        life.Value = 5;
        numeroMuertes = 0;
        points = 0;
        isDie = false;

        originRender = GetComponentInChildren<Renderer>();
        originMaterial = originRender.material;
    }

    private void Update()
    {
        if (life.Value <= 0 && !isDie)
        {
            Debug.Log("jugador eliminado");
            numeroMuertes++;
            isDie = true;

            // Iniciar cooldown para reactivar al jugador
            if (cooldownCoroutine == null)
            {
                cooldownCoroutine = StartCoroutine(CooldownCoroutine());
            }
        }
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
        ID.Value = (int)GetComponent<NetworkObject>().OwnerClientId;

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

    private IEnumerator CooldownCoroutine()
    {
        Debug.Log("Jugador eliminado. Iniciando cooldown de 10 segundos.");
        SetPlayerTag(false);

        yield return new WaitForSeconds(10);

        life.Value = 5; // Reiniciamos la vida para simplificar el ejemplo
        isDie = false;
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
        Debug.Log($"Vida del jugador despu�s del da�o: {life.Value}");

        if (life.Value <= 0)
        {
            // Aqu� puedes manejar la destrucci�n del coche o el reset de vida
            Debug.Log("Jugador eliminado");
            life.Value = 0; // Reiniciamos la vida para simplificar el ejemplo
        }
    }
}
