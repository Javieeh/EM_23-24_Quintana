using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Speedometer : MonoBehaviour
{
    public Rigidbody target;
    public CarController carController; // Referencia al coche, para obtener su velocidad actual    
    public float maxSpeed = 0.0f; // La velocidad máxima en km/h

    public float minSpeedArrowAngle;
    public float maxSpeedArrowAngle;

    [Header("UI")]
    public TextMeshProUGUI speedLabel; // El texto que muestra la velocidad
    public RectTransform arrow; // La flecha del velocimetro

    private float speed = 0.0f;
    private void Update()
    {
        speed = carController._currentSpeed * 3.6f; // Multiplicamos por 3.6 para obtener la velocidad en Km/h

        if (speedLabel != null)
            speedLabel.text = ((int)speed) + " km/h";
        if (arrow != null)
            arrow.localEulerAngles =
                new Vector3(0, 0, Mathf.Lerp(minSpeedArrowAngle, maxSpeedArrowAngle, speed / maxSpeed));
    }
}