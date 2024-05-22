using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ColorSeleccion : MonoBehaviour
{
    public Material materialCoche; // Referencia al material del coche que cambiar� de color
    public Color[] coloresDisponibles; // Lista de colores disponibles para la selecci�n
    private int indiceColorActual = 0; // �ndice del color actualmente seleccionado

    public Button botonAvanzar; // Referencia al bot�n de avanzar
    public Button botonRetroceder; // Referencia al bot�n de retroceder
    public Button botonConfirmar; // Referencia al bot�n de confirmar selecci�n

    public InputField inputNombreJugador; // Referencia al campo de entrada para el nombre del jugador

    // M�todo para inicializar el color del coche
    void Start()
    {
        CambiarColor();

        // Asignar las funciones a los botones
        botonAvanzar.onClick.AddListener(AvanzarColor);
        botonRetroceder.onClick.AddListener(RetrocederColor);
        botonConfirmar.onClick.AddListener(ConfirmarSeleccion);
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
        nuevoJugador.Name = nombreJugador;
        nuevoJugador.CarColor = colorCoche;
        //nuevoJugador.car = materialCoche.gameObject;

        // A�adir el nuevo jugador al GameManager
        GameManager.Instance.AddPlayer(nuevoJugador);

        // Cambiar a la siguiente escena
        SceneManager.LoadScene("NombreDeLaSiguienteEscena");
    }
}
