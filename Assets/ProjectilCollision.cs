using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ProjectilCollision : NetworkBehaviour
{
    // Start is called before the first frame update

    private float timeHit;

    public ulong shooterId;

    void Start()
    {
        timeHit = 0.1f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Dado");

            var player = other.GetComponentInParent<Player>();
            if (player != null)
            {
                if (IsServer)
                {
                    player.TakeDamageServerRpc(1, shooterId);
                }
                else
                {
                    RequestTakeDamageServerRpc(player.NetworkObjectId, 1, shooterId);
                }
            }

            if (IsServer)
            {
                NetworkObject.Despawn();
            }
            else
            {
                RequestDespawnServerRpc();
            }
            ////cogemos el componente de la vida de nuestro player y la reducimos en 1
            //other.GetComponentInParent<Player>().life--;

            ////Realizamos un cambio de color muy breve para darle feeling a la partida
            //// Cambiar el material del objeto del jugador
            //Renderer playerRenderer = other.GetComponent<Renderer>();
            //if (playerRenderer != null && hitMaterial != null)
            //{
            //    // Almacenar el material original del jugador
            //    Material originalMaterial = playerRenderer.material;
            //    Debug.Log("Material original: " + originalMaterial.name);

            //    // Cambiar el material al material de impacto
            //    playerRenderer.material = hitMaterial;
            //    Debug.Log("Material hit: " + hitMaterial.name);

            //    // Volver al material original después de materialDuration segundos
            //    StartCoroutine(ResetMaterial(originalMaterial, playerRenderer, other.GetComponent<Player>().life, timeHit));
            //}

            ////Destruimos la bala para que no siga impactando 0.01 segundos despues para que se pueda realizar la coroutina


            //Destroy(gameObject, timeHit + 0.01f);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestDespawnServerRpc()
    {
        DespawnBullet();
    }

    void DespawnBullet()
    {
        var networkObject = GetComponent<NetworkObject>();
        if (networkObject != null && networkObject.IsSpawned)
        {
            networkObject.Despawn();
        }
        else
        {
            Debug.LogWarning("Trying to despawn a bullet that is not spawned");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestTakeDamageServerRpc(ulong playerNetworkObjectId, int damage, ulong shooterId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkObjectId, out var networkObject))
        {
            var player = networkObject.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamageServerRpc(damage, shooterId);
            }
        }
    }
}
