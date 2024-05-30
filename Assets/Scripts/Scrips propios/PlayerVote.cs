using UnityEngine;
using Unity.Netcode;

public class PlayerVote : Singleton<PlayerVote>
{
    private NetworkVariable<int[]> mapVotes = new NetworkVariable<int[]>(new int[4]);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    [ServerRpc(RequireOwnership = false)]
    public void VoteForMapServerRpc(int mapIndex)
    {
        if (mapIndex < 0 || mapIndex >= mapVotes.Value.Length)
        {
            Debug.LogError("Invalid map index");
            return;
        }

        mapVotes.Value[mapIndex]++;
        mapVotes.SetDirty(true); // Marca la variable como sucia para asegurar que se sincronice
        UpdateVotesClientRpc(mapVotes.Value);
    }

    [ClientRpc]
    private void UpdateVotesClientRpc(int[] updatedVotes)
    {
        UIManager.Instance.UpdateMapVotes(updatedVotes);
    }
}

