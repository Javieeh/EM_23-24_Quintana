using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    private Transform carTransform;

    private void Start()
    {
        if (!IsOwner) return;

        SceneManager.sceneLoaded += OnSceneLoaded;

        // Asignar el Transform del coche
        carTransform = transform.Find("car");

        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (IsOwner)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsOwner) return;

        if (scene.name == "NascarC")
        {
            // Mover el jugador a la posición de inicio en la escena del circuito
            Transform startPosition = PlayersManager.Instance.GetStartPosition(NetworkManager.Singleton.LocalClientId);
            if (startPosition != null && carTransform != null)
            {
                carTransform.position = startPosition.position;
                carTransform.rotation = startPosition.rotation;
            }
        }
    }
}
