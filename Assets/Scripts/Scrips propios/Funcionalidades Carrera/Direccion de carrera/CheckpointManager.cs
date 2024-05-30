using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public List<Checkpoint> checkpoints; // Lista de checkpoints en el orden correcto
    private int currentCheckpointIndex = 0;
    private int currentLap = 0;
    public int totalLaps = 3; // N�mero total de vueltas
    private bool raceCompleted = false;

    private void Start()
    {
        if (checkpoints.Count == 0)
        {
            Debug.LogError("No checkpoints assigned in CheckpointManager.");
        }
    }

    public void CheckpointReached(Checkpoint checkpoint)
    {
        if (raceCompleted) return;

        if (currentCheckpointIndex < checkpoints.Count && checkpoints[currentCheckpointIndex] == checkpoint)
        {
           Debug.Log("Checkpoint " + currentCheckpointIndex + " reached");
            currentCheckpointIndex++;

            if (currentCheckpointIndex >= checkpoints.Count)
            {
               Debug.Log("All checkpoints reached for this lap. You can now finish the lap!");
            }
        }
        else
        {
            Debug.Log("Wrong checkpoint. Continue the race in order.");
        }
    }

    public void FinishLineReached()
    {
        if (currentCheckpointIndex >= checkpoints.Count)
        {
            currentLap++;
            Debug.Log("Lap " + currentLap + " completed!");

            if (currentLap >= totalLaps)
            {
                raceCompleted = true;
                Debug.Log("You have completed the race!");
            }
            else
            {
                currentCheckpointIndex = 0; // Reiniciar el �ndice de checkpoints para la nueva vuelta
                Debug.Log("Starting lap " + (currentLap + 1));
            }
        }
        else
        {
            Debug.Log("You need to pass all checkpoints before finishing the lap.");
        }
    }
}