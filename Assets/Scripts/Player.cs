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
    public NetworkVariable<int> CurrentPosition = new NetworkVariable<int>(default, readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Server);
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
    private ulong lastHitBy;
    private Coroutine cooldownCoroutine;

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
        UpdatePlayerPositionsClientRpc();
    }
    [ClientRpc]
    private void UpdatePlayerPositionsClientRpc()
    {
        if (IsOwner && GetComponentInChildren<CarController>().enabled)
        {
            Debug.Log("Player " + ID.Value + " ENTRA");
            UIManager.Instance.UpdateAllPlayerPositions(CurrentPosition.Value, PlayersManager.Instance.PlayersInGame);
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

        // Aumentar el contador de muertes en todos los clientes
        IncrementarMuertesClientRpc();
        // Incrementar el contador de destrucciones del jugador atacante
        IncrementarDestruccionesClientRpc(lastHitBy);

        int cooldownTime = 10;
        for (int i = cooldownTime; i > 0; i--)
        {
            ShowCooldownTextClientRpc(i);
            yield return new WaitForSeconds(1);
        }

        HideCooldownTextClientRpc();

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
    public void TakeDamageServerRpc(int damage, ulong attackerId)
    {
        if (!IsServer || cooldownCoroutine != null)
            return;

        life.Value -= damage;
        lastHitBy = attackerId;
        Debug.Log($"Vida del jugador después del daño: {life.Value}");

        if (life.Value <= 0)
        {
            if (cooldownCoroutine == null)
            {
                cooldownCoroutine = StartCoroutine(CooldownCoroutine());
            }
        }
    }



    [ClientRpc]
    private void IncrementarMuertesClientRpc()
    {
        if (IsOwner)
        {
            CombatGUI.Instance.IncrementarMuertes();
        }
        else
        {
            Debug.LogError("UIManager.Instance is null.");
        }
    }

    [ClientRpc]
    private void IncrementarDestruccionesClientRpc(ulong attackerId)
    {
        if (NetworkManager.Singleton.LocalClientId == attackerId)
        {
            if (CombatGUI.Instance != null)
            {
                CombatGUI.Instance.IncrementarDestrucciones();
            }
            else
            {
                Debug.LogError("UIManager.Instance is null.");
            }
        }
    }

    [ClientRpc]
    private void ShowCooldownTextClientRpc(int seconds)
    {
        if (IsOwner)
        {
            CombatGUI.Instance.ShowCooldownText(seconds);
        }
        else
        {
            Debug.LogError("UIManager.Instance is null.");
        }
    }

    [ClientRpc]
    private void HideCooldownTextClientRpc()
    {
        if (IsOwner)
        {
            CombatGUI.Instance.HideCooldownText();
        }
        else
        {
            Debug.LogError("UIManager.Instance is null.");
        }
    }
}
