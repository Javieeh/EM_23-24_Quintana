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
    private NetworkVariable<int> nascarVotes = new NetworkVariable<int>();
    private NetworkVariable<int> oasisVotes = new NetworkVariable<int>();
    private NetworkVariable<int> owlPlainsVotes = new NetworkVariable<int>();
    private NetworkVariable<int> rainyVotes = new NetworkVariable<int>();


    // Start is called before the first frame update
    void Start()
    {
        //nascarVotes.Value = 0;
        //oasisVotes.Value = 0;
        //owlPlainsVotes.Value = 0;
        //rainyVotes.Value = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    

    //PlayerVote
    [ServerRpc(RequireOwnership = false)]
    public void VoteForMapServerRpc(int mapIndex)
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

            default:
                break;
        }

        UpdateVotesClientRpc(mapIndex); //actualizamos
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

            default:
                break;
        }

        UIManager.Instance.UpdateMapVotes(updatedVote, mapIndex);
    }

    public int GetWinningMap() //Metodo para elegir el mapa ganador
    {
        
        int winningMap = Math.Max(Math.Max(nascarVotes.Value, oasisVotes.Value), Math.Max(owlPlainsVotes.Value, rainyVotes.Value));
        return winningMap;
    }
}
