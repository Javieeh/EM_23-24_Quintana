using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class VotingManager : Singleton<VotingManager>
{
    private NetworkVariable<int> nascarVotes = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> oasisVotes = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> owlPlainsVotes = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> rainyVotes = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private Dictionary<ulong, int> playerVotes = new Dictionary<ulong, int>();

    void Start()
    {
        nascarVotes.OnValueChanged += OnVotesChanged;
        oasisVotes.OnValueChanged += OnVotesChanged;
        owlPlainsVotes.OnValueChanged += OnVotesChanged;
        rainyVotes.OnValueChanged += OnVotesChanged;
    }

    void OnDestroy()
    {
        nascarVotes.OnValueChanged -= OnVotesChanged;
        oasisVotes.OnValueChanged -= OnVotesChanged;
        owlPlainsVotes.OnValueChanged -= OnVotesChanged;
        rainyVotes.OnValueChanged -= OnVotesChanged;
    }

    private void OnVotesChanged(int previousValue, int newValue)
    {
        UpdateAllVotesClientRpc(nascarVotes.Value, oasisVotes.Value, owlPlainsVotes.Value, rainyVotes.Value);
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
        // Inmediatamente envía los votos actualizados a todos los clientes
        UpdateAllVotesClientRpc(nascarVotes.Value, oasisVotes.Value, owlPlainsVotes.Value, rainyVotes.Value);
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
    private void UpdateAllVotesClientRpc(int nascarVotes, int oasisVotes, int owlPlainsVotes, int rainyVotes)
    {
        UIManager.Instance.UpdateMapVotes(nascarVotes, 0);
        UIManager.Instance.UpdateMapVotes(oasisVotes, 1);
        UIManager.Instance.UpdateMapVotes(owlPlainsVotes, 2);
        UIManager.Instance.UpdateMapVotes(rainyVotes, 3);
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
