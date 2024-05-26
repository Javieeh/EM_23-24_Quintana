using UnityEngine;

public class WheelCollisionDetector : MonoBehaviour
{
    private CarController carController;

    private void Start()
    {
        carController = GetComponentInParent<CarController>();
        if (carController == null)
        {
            Debug.LogError("CarController not found in parent.");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        
        if (collision.collider.CompareTag("Carretera"))
        {
            carController.SetLastRoadSegment(collision.collider.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Carretera"))
        {
            carController.SetLastRoadSegment(other.gameObject);
        }
    }
}