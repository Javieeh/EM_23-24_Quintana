using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public GameObject CanvasSeleccion;
    public GameObject CanvasEmparejamiento;

    public Material materialCoche; // Referencia al material del coche que cambiará de color
    public Color[] coloresDisponibles; // Lista de colores disponibles para la selección
    private int indiceColorActual = 0; // Índice del color actualmente seleccionado

    public Button botonAvanzar; // Referencia al botón de avanzar
    public Button botonRetroceder; // Referencia al botón de retroceder
    public Button botonConfirmar; // Referencia al botón de confirmar selección

    public InputField inputNombreJugador; // Referencia al campo de entrada para el nombre del jugador
    public Text playersListText;

    private Network_Connection_Manager networkConnectionManager;
    private List<string> playerNames = new List<string>(); //lista de jugadores

    // Método para inicializar el color del coche
    void Start()
    {
        CanvasSeleccion.SetActive(true);
        CanvasEmparejamiento.SetActive(false);

        CambiarColor();

        // Asignar las funciones a los botones
        botonAvanzar.onClick.AddListener(AvanzarColor);
        botonRetroceder.onClick.AddListener(RetrocederColor);
        botonConfirmar.onClick.AddListener(ConfirmarSeleccion);


        networkConnectionManager = FindObjectOfType<Network_Connection_Manager>();


        if (networkConnectionManager == null)
        {
            Debug.LogError("NetworkConnectionManager not found in the scene.");
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
    
    }

    // Método para cambiar el color del coche
    void CambiarColor()
    {
        if (coloresDisponibles.Length > 0)
        {
            materialCoche.color = coloresDisponibles[indiceColorActual];
        }
    }

    // Método para avanzar en la selección de colores
    public void AvanzarColor()
    {
        indiceColorActual = (indiceColorActual + 1) % coloresDisponibles.Length;
        CambiarColor();
    }

    // Método para retroceder en la selección de colores
    public void RetrocederColor()
    {
        indiceColorActual = (indiceColorActual - 1 + coloresDisponibles.Length) % coloresDisponibles.Length;
        CambiarColor();
    }

    public void ConfirmarSeleccion()
    {
        string nombreJugador = inputNombreJugador.text;
        Color colorCoche = coloresDisponibles[indiceColorActual];

        // Crear un nuevo Player y configurarlo
        GameObject playerObj = new GameObject("Player");
        Player nuevoJugador = playerObj.AddComponent<Player>();
        nuevoJugador.Name = nombreJugador;
        nuevoJugador.CarColor = colorCoche;
        //nuevoJugador.car = materialCoche.gameObject;

        CanvasSeleccion.SetActive(false);
        CanvasEmparejamiento.SetActive(true);

        if (networkConnectionManager != null)
        {
            if (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
            {
                networkConnectionManager.StartHost();
            }
            else
            {
                networkConnectionManager.StartClient();
            }
        }





        // Añadir el nuevo jugador al GameManager
        GameManager.Instance.AddPlayer(nuevoJugador);

        // Actualizamos la lista de jugadores
        UpdatePlayersList();
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            string playerName = inputNombreJugador.text;
            playerNames.Add(playerName);
            UpdatePlayersList();
        }
    }
    private void OnClientDisconnectCallback(ulong clientId)
    {
        // Implementa lógica para manejar la desconexión si es necesario
    }

    private void UpdatePlayersList()
    {
        playersListText.text = "Players:\n";
        foreach (string name in playerNames)
        {
            playersListText.text += name + "\n";
        }
    }

    private void OnDestroy()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
    }










    }

  

   





