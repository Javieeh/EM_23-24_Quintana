using UnityEngine;
using Unity.Netcode;

public class CheckpointManager : NetworkBehaviour
{
    public NetworkVariable<int> CurrentCheckpoint = new NetworkVariable<int>(0);
    public NetworkVariable<int> LapsCompleted = new NetworkVariable<int>(0);
    private int TotalCheckpoints;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint")) // Si pasa por un checkpoint...
        {
            Checkpoint checkpoint = other.GetComponent<Checkpoint>();
            if (checkpoint != null) // Si es un checkpoint valido
            {   // Si el checkpoint es el inicial (0) y el valor actual del checkpoint es el ultimo. Habremos completado una vuelta
                if (checkpoint.CheckpointNumber == 0 && CurrentCheckpoint.Value == TotalCheckpoints) 
                {   
                    LapsCompleted.Value += 1;
                } // Si no, simplemente guardamos que hemos pasado por ese checkpoint
                CurrentCheckpoint.Value = checkpoint.CheckpointNumber;
            }
        }
    }

    #region cod_antiguo
    /*public List<Checkpoint> checkpoints; // Lista de checkpoints en el orden correcto
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
    }*/
    #endregion
}