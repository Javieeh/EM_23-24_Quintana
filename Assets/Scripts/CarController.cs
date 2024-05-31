using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI; // Aï¿½adir esta lï¿½nea
using UnityEngine;

public class CarController : MonoBehaviour
{
    #region Variables

    [Header("Movement")] public List<AxleInfo> axleInfos;
    [SerializeField] private float forwardMotorTorque = 100000;
    [SerializeField] private float backwardMotorTorque = 50000;
    [SerializeField] private float maxSteeringAngle = 15;
    [SerializeField] private float engineBrake = 1e+12f;
    [SerializeField] private float footBrake = 1e+24f;
    [SerializeField] private float topSpeed = 200f;
    [SerializeField] private float downForce = 100f;
    [SerializeField] private float slipLimit = 0.2f;
    [SerializeField] private float penaltyTime = 1f; // Tiempo de penalizaciï¿½n en segundos

    private float CurrentRotation { get; set; }
    public float InputAcceleration { get; set; }
    public float InputSteering { get; set; }
    public float InputBrake { get; set; }

    //private PlayerInfo m_PlayerInfo;

    private Rigidbody _rigidbody;
    private float _steerHelper = 0.8f;

    //Detecciï¿½n de colisiones
    private bool isPenalized = false;
    public Image fadeImage; // Aï¿½adir referencia a la imagen de fundido
    private GameObject lastRoadSegment; // para calcular la posicion tras haberse producido la colisiï¿½n
    private RespawnInfo lastRespawnInfo;

    public Text wrongWayText;

    //Direcciï¿½n de carrera
    private CheckpointManager checkpointManager;

    public float _currentSpeed = 0;



    //Disparar
    public Transform projectileSpawn;
    public GameObject projectilePrefab;
    public float projectileSpeed = 20f;
    public float projectileLife = 5f;

    private float Speed
    {
        get => _currentSpeed;
        set
        {
            if (Math.Abs(_currentSpeed - value) < float.Epsilon) return;
            _currentSpeed = value;
            if (OnSpeedChangeEvent != null)
                OnSpeedChangeEvent(_currentSpeed);
        }
    }

    public delegate void OnSpeedChangeDelegate(float newVal);

    public event OnSpeedChangeDelegate OnSpeedChangeEvent;

    #endregion Variables

    #region Unity Callbacks

    public int id;

    public void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        id = this.GetComponentInParent<Player>().id;
        
        /*checkpointManager = FindObjectOfType<CheckpointManager>();
        wrongWayText.gameObject.SetActive(false);

        if (checkpointManager == null)
        {
            Debug.LogError("CheckpointManager not found in the scene.");
        }
        */
    }


    public void SetLastRespawnInfo(RespawnInfo respawnInfo)
    {
        lastRespawnInfo = respawnInfo;
    }

    public void SetLastRoadSegment(GameObject roadSegment)
    {
        lastRoadSegment = roadSegment;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            StartCoroutine(ApplyPenalty());
        }

        Speed = _rigidbody.velocity.magnitude;

        CheckWrongWay();
    }

    public void FixedUpdate()
    {
        // Detectar si el coche estï¿½ volcado o de canto
        if (!isPenalized && IsCarInUnstablePosition())
        {
            StartCoroutine(ApplyPenalty());

        }

        InputSteering = Mathf.Clamp(InputSteering, -1, 1);
        InputAcceleration = Mathf.Clamp(InputAcceleration, -1, 1);
        InputBrake = Mathf.Clamp(InputBrake, 0, 1);

        float steering = maxSteeringAngle * InputSteering;

        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }

            if (axleInfo.motor)
            {
                if (InputAcceleration > float.Epsilon)
                {
                    axleInfo.leftWheel.motorTorque = forwardMotorTorque;
                    axleInfo.leftWheel.brakeTorque = 0;
                    axleInfo.rightWheel.motorTorque = forwardMotorTorque;
                    axleInfo.rightWheel.brakeTorque = 0;
                }

                if (InputAcceleration < -float.Epsilon)
                {
                    axleInfo.leftWheel.motorTorque = -backwardMotorTorque;
                    axleInfo.leftWheel.brakeTorque = 0;
                    axleInfo.rightWheel.motorTorque = -backwardMotorTorque;
                    axleInfo.rightWheel.brakeTorque = 0;
                }

                if (Math.Abs(InputAcceleration) < float.Epsilon)
                {
                    axleInfo.leftWheel.motorTorque = 0;
                    axleInfo.leftWheel.brakeTorque = engineBrake;
                    axleInfo.rightWheel.motorTorque = 0;
                    axleInfo.rightWheel.brakeTorque = engineBrake;
                }

                if (InputBrake > 0)
                {
                    axleInfo.leftWheel.brakeTorque = footBrake;
                    axleInfo.rightWheel.brakeTorque = footBrake;
                }
            }

            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }

        SteerHelper();
        SpeedLimiter();
        AddDownForce();
        TractionControl();
    }
    private bool IsCarInUnstablePosition()
    {
        // Consideramos que el coche estï¿½ en una posiciï¿½n inestable si estï¿½ inclinado mï¿½s de 45 grados en cualquier direcciï¿½n
        float angleThreshold = 0.7f; // Cosine of 45 degrees is approximately 0.7

        // Verificar si el coche estï¿½ "volcado" (upside down)
        if (Vector3.Dot(transform.up, Vector3.up) < -angleThreshold)
        {
            return true;
        }

        // Verificar si el coche estï¿½ de lado (cualquiera de las dos direcciones laterales)
        if (Mathf.Abs(Vector3.Dot(transform.right, Vector3.up)) > angleThreshold)
        {
            return true;
        }

        return false;
    }

    #region Colisiones y reaparicion
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle") && !isPenalized)
        {
            StartCoroutine(ApplyPenalty());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle") && !isPenalized)
        {
            StartCoroutine(ApplyPenalty());
        }

        if (other.CompareTag("Carretera"))
        {

            RespawnInfo respawnInfo = other.GetComponent<RespawnInfo>();
            if (respawnInfo != null)
            {
                SetLastRespawnInfo(respawnInfo);
            }
        }

        // Comprobaciï¿½n de checkpoints
        Checkpoint checkpoint = other.GetComponent<Checkpoint>();
        if (checkpoint != null && checkpointManager != null)
        {
            checkpointManager.CheckpointReached(checkpoint);
        }

        // Comprobaciï¿½n de la lï¿½nea de meta
        if (other.CompareTag("Finish") && checkpointManager != null)
        {
            checkpointManager.FinishLineReached();
        }


    }

    private IEnumerator ApplyPenalty()
    {
        isPenalized = true;
        yield return StartCoroutine(FadeToBlack());

        // Reposicionar el coche aquï¿½
        RespawnCar();

        yield return new WaitForSeconds(penaltyTime);
        yield return StartCoroutine(FadeToClear());
        isPenalized = false;
    }
    private IEnumerator FadeToBlack()
    {

        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < penaltyTime)
        {
            color.a = Mathf.Lerp(0, 1, elapsedTime / penaltyTime);
            fadeImage.color = color;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        color.a = 1;
        fadeImage.color = color;
    }

    private IEnumerator FadeToClear()
    {
        float fadeDuration = 1f;
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            color.a = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            fadeImage.color = color;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        color.a = 0;
        fadeImage.color = color;
    }

    private void RespawnCar()
    {

        Vector3 respawnPosition = lastRespawnInfo.respawnPoint.position;
        Quaternion respawnRotation = lastRespawnInfo.respawnPoint.rotation;

        _rigidbody.position = respawnPosition;
        _rigidbody.rotation = respawnRotation;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

    }

    #endregion

    private void CheckWrongWay()
    {
        if (lastRespawnInfo == null || lastRespawnInfo.respawnPoint == null) return;

        Vector3 directionToRespawn = lastRespawnInfo.respawnPoint.forward;
        float angle = Vector3.Angle(transform.forward, directionToRespawn);

        if (angle > 90f)
        {
            if (wrongWayText != null)
            {
                wrongWayText.gameObject.SetActive(true);
                Debug.Log("You are going the wrong way!");
            }
        }
        else
        {
            if (wrongWayText != null)
            {
                wrongWayText.gameObject.SetActive(false);
            }
        }
    }

    #endregion

    #region Methods

    // crude traction control that reduces the power to wheel if the car is wheel spinning too much
    private void TractionControl()
    {
        foreach (var axleInfo in axleInfos)
        {
            WheelHit wheelHitLeft;
            WheelHit wheelHitRight;
            axleInfo.leftWheel.GetGroundHit(out wheelHitLeft);
            axleInfo.rightWheel.GetGroundHit(out wheelHitRight);

            if (wheelHitLeft.forwardSlip >= slipLimit)
            {
                var howMuchSlip = (wheelHitLeft.forwardSlip - slipLimit) / (1 - slipLimit);
                axleInfo.leftWheel.motorTorque -= axleInfo.leftWheel.motorTorque * howMuchSlip * slipLimit;
            }

            if (wheelHitRight.forwardSlip >= slipLimit)
            {
                var howMuchSlip = (wheelHitRight.forwardSlip - slipLimit) / (1 - slipLimit);
                axleInfo.rightWheel.motorTorque -= axleInfo.rightWheel.motorTorque * howMuchSlip * slipLimit;
            }
        }
    }

    // this is used to add more grip in relation to speed
    private void AddDownForce()
    {
        foreach (var axleInfo in axleInfos)
        {
            axleInfo.leftWheel.attachedRigidbody.AddForce(
                -transform.up * (downForce * axleInfo.leftWheel.attachedRigidbody.velocity.magnitude));
        }
    }

    private void SpeedLimiter()
    {
        float speed = _rigidbody.velocity.magnitude;
        if (speed > topSpeed)
            _rigidbody.velocity = topSpeed * _rigidbody.velocity.normalized;
    }

    // finds the corresponding visual wheel
    // correctly applies the transform
    public void ApplyLocalPositionToVisuals(WheelCollider col)
    {
        if (col.transform.childCount == 0)
        {
            return;
        }

        Transform visualWheel = col.transform.GetChild(0);
        Vector3 position;
        Quaternion rotation;
        col.GetWorldPose(out position, out rotation);
        var myTransform = visualWheel.transform;
        myTransform.position = position;
        myTransform.rotation = rotation;
    }

    private void SteerHelper()
    {
        foreach (var axleInfo in axleInfos)
        {
            WheelHit[] wheelHit = new WheelHit[2];
            axleInfo.leftWheel.GetGroundHit(out wheelHit[0]);
            axleInfo.rightWheel.GetGroundHit(out wheelHit[1]);
            foreach (var wh in wheelHit)
            {
                if (wh.normal == Vector3.zero)
                    return; // wheels arent on the ground so dont realign the rigidbody velocity
            }
        }

        // this if is needed to avoid gimbal lock problems that will make the car suddenly shift direction
        if (Mathf.Abs(CurrentRotation - transform.eulerAngles.y) < 10f)
        {
            var turnAdjust = (transform.eulerAngles.y - CurrentRotation) * _steerHelper;
            Quaternion velRotation = Quaternion.AngleAxis(turnAdjust, Vector3.up);
            _rigidbody.velocity = velRotation * _rigidbody.velocity;
        }

        CurrentRotation = transform.eulerAngles.y;
    }


    public void Shoot()
    {
        // Instancia el proyectil en la posición y rotación del punto de origen
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawn.position, projectileSpawn.rotation);

        projectile.GetComponent<ProjectilCollision>().id = id;

        // Añade una fuerza al proyectil para que se mueva en la dirección del coche
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = projectileSpawn.forward * projectileSpeed;
        }

        // Destruye el proyectil después de projectileLifetime segundos
        Destroy(projectile, projectileLife);

        Debug.Log("Shooting from the car!");
    }

    #endregion
}