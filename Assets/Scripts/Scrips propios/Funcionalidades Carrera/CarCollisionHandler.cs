using System.Collections;
using UnityEngine;

public class CarCollisionHandler : MonoBehaviour
{
    public float penaltyTime = 3f; // Tiempo de penalizaci�n en segundos
    private bool isPenalized = false;
    private CarController carController; // Referencia al script que controla el coche

    void Start()
    {
        carController = GetComponent<CarController>();
        if (carController == null)
        {
            Debug.LogError("CarController script not found on the car.");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle") && !isPenalized)
        {
            StartCoroutine(ApplyPenalty());
        }
    }

    IEnumerator ApplyPenalty()
    {
        isPenalized = true;
        carController.enabled = false; // Desactivar el control del coche durante la penalizaci�n
        yield return new WaitForSeconds(penaltyTime);
        carController.enabled = true; // Reactivar el control del coche despu�s de la penalizaci�n
        isPenalized = false;
    }
}