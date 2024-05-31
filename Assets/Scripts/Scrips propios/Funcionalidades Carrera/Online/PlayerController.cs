using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    private void Start()
    {
        if (!IsOwner) return;

        SceneManager.sceneLoaded += OnSceneLoaded;
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
        if (scene.name == "CircuitScene")
        {
            // Mover el jugador a la posición de inicio en la escena del circuito
            Transform startPosition = GameObject.Find("StartPosition").transform;
            transform.position = startPosition.position;
            transform.rotation = startPosition.rotation;
        }
    }
}
