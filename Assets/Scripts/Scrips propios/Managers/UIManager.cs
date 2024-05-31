using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public TextMeshProUGUI[] playerPosTexts;
    private Speedometer speedometer;
    [SerializeField] private Button startServerButton;
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;

    [SerializeField] private TextMeshProUGUI playersInGameText;


    private void Awake()
    {
        Cursor.visible = true;
        Instance = this;
        speedometer = FindObjectOfType<Speedometer>();
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

                PlayersManager playerManager = new PlayersManager();
                if (playerManager != null)
                {
                    // Llamar al m�todo SpawnPlayer de PlayersManager con el �ndice del placeholder
                    playerManager.SpawnPlayer(NetworkManager.Singleton.LocalClientId); // Por ejemplo, el primer placeholder
                }
                else
                {
                    Debug.LogError("PlayersManager instance is null.");
                }
                /*// CarController
                CarController[] carControllers = FindObjectsOfType<CarController>();

                foreach (CarController carController in carControllers)
                {
                    NetworkObject networkObject = carController.gameObject.GetComponentInParent<NetworkObject>();
                    if (networkObject.IsOwner && networkObject != null)
                    {
                        speedometer._carController = carController;
                        speedometer._target = speedometer._carController.gameObject.GetComponent<Rigidbody>();
                    }
                }*/
            }
            else
            {
                Debug.LogError("Host could not be started...");
            }
        });
        startServerButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartServer())
            {
                Console.WriteLine("Server started...");
            }
            else
            {
                Console.WriteLine("Server could not be started...");

            }
        }
        );
        startClientButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartClient())
            {
                Console.WriteLine("Client started...");
            }
            else
            {
                Console.WriteLine("Client could not be started...");

            }
        }
        );
    }




}



