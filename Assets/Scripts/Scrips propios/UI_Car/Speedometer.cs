using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class Speedometer : MonoBehaviour
{
    public Rigidbody _target;
    public CarController _carController; // Referencia al coche, para obtener su velocidad actual    
    public float maxSpeed = 0.0f; // La velocidad máxima en km/h

    public float minSpeedArrowAngle;
    public float maxSpeedArrowAngle;

    [Header("UI")]
    public TextMeshProUGUI speedLabel; // El texto que muestra la velocidad
    public RectTransform arrow; // La flecha del velocimetro

    private float speed = 0.0f;

    private void Start() 
    {
       
    }
    private void Update()
    {    
        
    }

    private void OnEnable()
    {
        CarController[] carControllers = FindObjectsOfType<CarController>();
        Rigidbody[] rigidBodies = FindObjectsOfType<Rigidbody>();
        foreach (CarController carController in carControllers)
        {
            if (carController.gameObject.GetComponentInParent<NetworkObject>().IsOwner)
            {
                _carController = carController;
            }
        }
        foreach (Rigidbody rigidbody in rigidBodies)
        {
            if (rigidbody.gameObject.GetComponentInParent<NetworkObject>().IsOwner)
            {
                _target = rigidbody;
            }
        }
    }
}