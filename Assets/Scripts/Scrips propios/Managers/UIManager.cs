using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class UIManager : MonoBehaviour
{
    [Header("INITIAL MENU")]
    [SerializeField] private GameObject initialMenu;
    [SerializeField] private Button startServerButton;
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;

    [Header("SELECTION MENU")]
    [SerializeField] private GameObject selectionMenu;
    [SerializeField] private TextMeshProUGUI playersInGameText;

    [Header("COLOR MENU")]
    [SerializeField] private Button nextColorButton;

    [Header("NAME MENU")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button setNameButton;

    private void Awake()
    {
        Cursor.visible = true;
    }

    private void Update()
    {
        playersInGameText.text = $"Players in game: {PlayersManager.Instance.PlayersInGame}";
    }

    void Start()
    {
        Screen.fullScreen = false;
        Screen.SetResolution(800, 600, false);

        startHostButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartHost())
            {
                Debug.Log("Host started...");
            }
            else
            {
                Debug.LogError("Host could not be started...");
            }
            initialMenu.SetActive(false);
            selectionMenu.SetActive(true);
        });

        startServerButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartServer())
            {
                Debug.Log("Server started...");
            }
            else
            {
                Debug.LogError("Server could not be started...");
            }
            initialMenu.SetActive(false);
            selectionMenu.SetActive(true);
        });

        startClientButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log("Client started...");
            }
            else
            {
                Debug.LogError("Client could not be started...");
            }
            initialMenu.SetActive(false);
            selectionMenu.SetActive(true);
        });

        nextColorButton.onClick.AddListener(() =>
        {
            // Obtener el jugador local y cambiar su color
            var localPlayerColor = FindLocalPlayer<PlayerColor>();
            if (localPlayerColor != null)
            {
                localPlayerColor.NextColor();
            }
        });

        setNameButton.onClick.AddListener(() =>
        {
            // Obtener el jugador local y cambiar su nombre
            var localPlayerName = FindLocalPlayer<PlayerName>();
            if (localPlayerName != null && !string.IsNullOrWhiteSpace(nameInputField.text))
            {
                localPlayerName.SetName(nameInputField.text);
            }
        });
    }

    private T FindLocalPlayer<T>() where T : NetworkBehaviour
    {
        var players = FindObjectsOfType<T>();
        foreach (var player in players)
        {
            if (player.IsOwner)
            {
                return player;
            }
        }
        return null;
    }
}
