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
    [SerializeField] private Button startServerButton;
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;

    [SerializeField] private TextMeshProUGUI playersInGameText;


    private void Awake()
    {
        Cursor.visible = true;
    }

    private void Update()
    {
        playersInGameText.text = $"Players in game: {PlayersManager.Instance.PlayersInGame}";
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
                    // Llamar al método SpawnPlayer de PlayersManager con el índice del placeholder
                    playerManager.SpawnPlayer(NetworkManager.Singleton.LocalClientId); // Por ejemplo, el primer placeholder
                }
                else
                {
                    Debug.LogError("PlayersManager instance is null.");
                }
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

    

