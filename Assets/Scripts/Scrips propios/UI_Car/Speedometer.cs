using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System.Collections;

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
        StartCoroutine(WaitToLink());
    }
    private void Update()
    {
        speed = _carController._currentSpeed.Value * 3.6f; // Multiplicamos por 3.6 para obtener la velocidad en Km/h

        if (speedLabel != null)
            speedLabel.text = ((int)speed) + " km/h";
        if (arrow != null)
            arrow.localEulerAngles =
                new Vector3(0, 0, Mathf.Lerp(minSpeedArrowAngle, maxSpeedArrowAngle, speed / maxSpeed));
    }
    private IEnumerator WaitToLink()
    {
        yield return new WaitForSeconds(1);
        _target = PlayersManager.Instance.GetRB();
        _carController = _target.transform.GetComponent<CarController>();
    }
}