using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int numPlayers = 50;

    public RaceController currentRace;

    public static GameManager Instance { get; private set; }

    public List<Player> players = new List<Player>();
    


    //Nombre de cada jugador
    public string playerName;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        StartCoroutine(StartCountDown());
    }

    public void AddPlayer(Player player)
    {
        players.Add(player);
    }

    public List<Player> GetPlayers()
    {
        return players;
    }
    private IEnumerator StartCountDown(){
        // Hacemos estaticos a los jugadores
        foreach (Player player in players) player.GetComponentInChildren<Rigidbody>().isKinematic = true;
        
        yield return new WaitForSeconds(1);

    }
}