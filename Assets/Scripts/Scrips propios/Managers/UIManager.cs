using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{


    [Header("INITIAL MENU")]
    [SerializeField] private GameObject initialMenu;
    [SerializeField] private Button startServerButton;
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;
    [SerializeField] private Button startLocalButton;

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
    [SerializeField] private Button fightMapButton;
    [SerializeField] private TextMeshProUGUI[] mapVoteTexts;

    [Header("SPEEDOMETER")]
    [SerializeField] public TextMeshProUGUI positions;
    private Speedometer speedometer;

    string [] mapsArray = { "nascarVotes", "oasisVotes", "owlPlainsVotes", "rainyVotes", "fightVotes" };

    private void Awake()
    {
        Cursor.visible = true;

    }

    private void Update()
    {
        playersInGameText.text = $"Players in game: {PlayersManager.Instance.PlayersInGame}";
    }

    public void InitPositionText(int playerIndex, int totalPlayers, TextMeshProUGUI positionText)
    {
        positions = positionText;
        if (playerIndex <= totalPlayers)
        {
            //positions.text = $"{playerIndex + 1}/{totalPlayers}";
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

        startLocalButton.onClick.AddListener(() => SceneManager.LoadScene("Practica"));

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
        fightMapButton.onClick.AddListener(() => VoteForMap(4));
    }

    private void VoteForMap(int mapIndex)
    {

        if (NetworkManager.Singleton.IsClient)
        {
            Debug.Log($"Voting for map {mapIndex}");
            VotingManager.Instance.VoteForMapServerRpc(mapIndex);
        }
    }

    public void UpdateMapVotes(int updatedVote, int mapIndex)
    {
        // Actualizar los textos de los botones de mapa con el número de votos
        Debug.Log("Updating map votes UI");
        mapVoteTexts[mapIndex].text = $"{updatedVote}";

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



