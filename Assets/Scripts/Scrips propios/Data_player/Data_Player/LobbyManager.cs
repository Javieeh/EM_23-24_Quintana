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

    public Material materialCoche; // Referencia al material del coche que cambiar� de color
    public Color[] coloresDisponibles; // Lista de colores disponibles para la selecci�n
    private int indiceColorActual = 0; // �ndice del color actualmente seleccionado

    public Button botonAvanzar; // Referencia al bot�n de avanzar
    public Button botonRetroceder; // Referencia al bot�n de retroceder
    public Button botonConfirmar; // Referencia al bot�n de confirmar selecci�n

    public InputField inputNombreJugador; // Referencia al campo de entrada para el nombre del jugador
    public Text playersListText;

    private Network_Connection_Manager networkConnectionManager;
    private List<string> playerNames = new List<string>(); //lista de jugadores

    // M�todo para inicializar el color del coche
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

    // M�todo para cambiar el color del coche
    void CambiarColor()
    {
        if (coloresDisponibles.Length > 0)
        {
            materialCoche.color = coloresDisponibles[indiceColorActual];
        }
    }

    // M�todo para avanzar en la selecci�n de colores
    public void AvanzarColor()
    {
        indiceColorActual = (indiceColorActual + 1) % coloresDisponibles.Length;
        CambiarColor();
    }

    // M�todo para retroceder en la selecci�n de colores
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
        nuevoJugador.Name.Value = nombreJugador;
        nuevoJugador.CarColor.Value = colorCoche;
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





        // A�adir el nuevo jugador al GameManager
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
        // Implementa l�gica para manejar la desconexi�n si es necesario
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

  

   





