using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{


    [Header("INITIAL MENU")]
    [SerializeField] private GameObject initialMenu;
    [SerializeField] private Button startServerButton;
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;

    [Header("SELECTION MENU")]
    [SerializeField] private GameObject selectionMenu;
    [SerializeField] private TextMeshProUGUI playersInGameText;
    [SerializeField] private Button readyButton; // Nuevo botón de "listo"

    [Header("COLOR MENU")]
    [SerializeField] private Button nextColorButton;

    [Header("NAME MENU")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button setNameButton;

    [Header("COUNTDOWN")]
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("MAP VOTING")]
    [SerializeField] private Button mapButton1;
    [SerializeField] private Button mapButton2;
    [SerializeField] private Button mapButton3;
    [SerializeField] private Button mapButton4;
    [SerializeField] private TextMeshProUGUI[] mapVoteTexts;

    [Header("SPEEDOMETER")]
    [SerializeField] public static UIManager Instance;
    [SerializeField] public TextMeshProUGUI[] playerPosTexts;
    private Speedometer speedometer;

    private void Awake()
    {
        Cursor.visible = true;

    }

    private void Update()
    {
        playersInGameText.text = $"Players in game: {PlayersManager.Instance.PlayersInGame}";
    }
    public void UpdatePlayerPosition(string playerName, int pos)
    {
        // Actualizamos el texto correspondiente en la interfaz
        if (pos - 1 < playerPosTexts.Length)
        {
            playerPosTexts[pos - 1].text = $"{pos}. {playerName}";
        }
    }
    // Start is called before the first frame update
    void Start()
    {
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
        }
        );
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
        }
        );
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

        readyButton.onClick.AddListener(() =>
        {
            var localPlayerReady = FindLocalPlayer<PlayerReady>();
            if (localPlayerReady != null)
            {
                localPlayerReady.SetReady();
            }
        });

        mapButton1.onClick.AddListener(() => VoteForMap(0));
        mapButton2.onClick.AddListener(() => VoteForMap(1));
        mapButton3.onClick.AddListener(() => VoteForMap(2));
        mapButton4.onClick.AddListener(() => VoteForMap(3));
    }

    private void VoteForMap(int mapIndex)
    {
        if (NetworkManager.Singleton.IsClient)
        {
            PlayerVote.Instance.VoteForMapServerRpc(mapIndex);
        }
    }

    public void UpdateMapVotes(int[] mapVotes)
    {
        // Actualizar los textos de los botones de mapa con el número de votos
        for (int i = 0; i < mapVoteTexts.Length; i++)
        {
            mapVoteTexts[i].text = $"Map {i + 1}: {mapVotes[i]} votes";
        }
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

    public void UpdateCountdownText(int timeRemaining)
    {
        if (countdownText != null)
        {
            countdownText.text = $"Game starts in: {timeRemaining} seconds";
        }
    }


}



