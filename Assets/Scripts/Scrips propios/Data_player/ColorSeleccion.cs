using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ColorSeleccion : MonoBehaviour
{
    public Material materialCoche; // Referencia al material del coche que cambiará de color
    public Color[] coloresDisponibles; // Lista de colores disponibles para la selección
    private int indiceColorActual = 0; // Índice del color actualmente seleccionado

    public Button botonAvanzar; // Referencia al botón de avanzar
    public Button botonRetroceder; // Referencia al botón de retroceder
    public Button botonConfirmar; // Referencia al botón de confirmar selección

    public InputField inputNombreJugador; // Referencia al campo de entrada para el nombre del jugador

    // Método para inicializar el color del coche
    void Start()
    {
        CambiarColor();

        // Asignar las funciones a los botones
        botonAvanzar.onClick.AddListener(AvanzarColor);
        botonRetroceder.onClick.AddListener(RetrocederColor);
        botonConfirmar.onClick.AddListener(ConfirmarSeleccion);
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

        // Añadir el nuevo jugador al GameManager
        GameManager.Instance.AddPlayer(nuevoJugador);

        // Cambiar a la siguiente escena
        SceneManager.LoadScene("NombreDeLaSiguienteEscena");
    }
}
