using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Collections;

public class PlayerController : NetworkBehaviour
{
    private void Start()
    {
        if (!IsOwner) return;

        Debug.Log("PlayerController Start - Owner: " + IsOwner);
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
        if (!IsOwner) return;

        Debug.Log("Scene Loaded: " + scene.name);

        if (scene.name == "Nascar")
        {
            // Asegurarse de que PlayersManager esté disponible y reposicionar al jugador
            StartCoroutine(PositionPlayerInCircuit());
        }
    }

    private IEnumerator PositionPlayerInCircuit()
    {
        // Esperar hasta que PlayersManager esté disponible
        yield return new WaitUntil(() => PlayersManager.Instance != null);

        Transform startTransform = PlayersManager.Instance.GetStartPosition(OwnerClientId);

        if (startTransform != null)
        {
            Debug.Log("Moving player to start position in CircuitScene");

            // Reposicionar el jugador y su hijo "car"
            transform.position = startTransform.position;
            transform.rotation = startTransform.rotation;

            Transform carTransform = transform.Find("car");
            if (carTransform != null)
            {
                carTransform.localPosition = Vector3.zero;
                carTransform.localRotation = Quaternion.identity;
            }
        }
        else
        {
            Debug.LogError("Start position not found for player.");
        }
    }
}
