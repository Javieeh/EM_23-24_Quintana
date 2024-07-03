using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class VotingManager : Singleton<VotingManager>
{
    private NetworkVariable<int> nascarVotes = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> oasisVotes = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> owlPlainsVotes = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> rainyVotes = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private Dictionary<ulong, int> playerVotes = new Dictionary<ulong, int>();

    void Start()
    {
        // Inicialización si es necesario
    }

    void Update()
    {
        // Lógica de actualización si es necesario
    }

    [ServerRpc(RequireOwnership = false)]
    public void VoteForMapServerRpc(int mapIndex, ServerRpcParams rpcParams = default)
    {
        ulong playerId = rpcParams.Receive.SenderClientId;

        if (playerVotes.ContainsKey(playerId))
        {
            int lastVote = playerVotes[playerId];
            DecreaseVote(lastVote);
        }

        IncreaseVote(mapIndex);
        playerVotes[playerId] = mapIndex;
        UpdateVotesClientRpc(mapIndex);
    }

    private void DecreaseVote(int mapIndex)
    {
        switch (mapIndex)
        {
            case 0:
                nascarVotes.Value--;
                break;
            case 1:
                oasisVotes.Value--;
                break;
            case 2:
                owlPlainsVotes.Value--;
                break;
            case 3:
                rainyVotes.Value--;
                break;
        }
    }

    private void IncreaseVote(int mapIndex)
    {
        switch (mapIndex)
        {
            case 0:
                nascarVotes.Value++;
                break;
            case 1:
                oasisVotes.Value++;
                break;
            case 2:
                owlPlainsVotes.Value++;
                break;
            case 3:
                rainyVotes.Value++;
                break;
        }
    }

    [ClientRpc]
    private void UpdateVotesClientRpc(int mapIndex)
    {
        int updatedVote = 0;
        switch (mapIndex)
        {
            case 0:
                updatedVote = nascarVotes.Value;
                break;
            case 1:
                updatedVote = oasisVotes.Value;
                break;
            case 2:
                updatedVote = owlPlainsVotes.Value;
                break;
            case 3:
                updatedVote = rainyVotes.Value;
                break;
        }

        UIManager.Instance.UpdateMapVotes(updatedVote, mapIndex);
    }

    public int GetWinningMap()
    {
        Debug.Log(nascarVotes.Value);
        Debug.Log(oasisVotes.Value);
        Debug.Log(owlPlainsVotes.Value);
        Debug.Log(rainyVotes.Value);

        int[] votesArray = { nascarVotes.Value, oasisVotes.Value, owlPlainsVotes.Value, rainyVotes.Value };
        int max = -1;
        int winningMap = -1;
        for (int i = 0; i < votesArray.Length; i++)
        {
            if (votesArray[i] > max)
            {
                max = votesArray[i];
                winningMap = i;
            }
        }
        Debug.Log("WINNING MAP: " + winningMap);
        return winningMap;
    }
}
