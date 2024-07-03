using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class CarController : NetworkBehaviour
{
    #region Variables

    [Header("Movement")]
    public List<AxleInfo> axleInfos;
    [SerializeField] private float forwardMotorTorque = 100000;
    [SerializeField] private float backwardMotorTorque = 50000;
    [SerializeField] private float maxSteeringAngle = 15;
    [SerializeField] private float engineBrake = 1e+12f;
    [SerializeField] private float footBrake = 1e+24f;
    [SerializeField] private float topSpeed = 200f;
    [SerializeField] private float downForce = 100f;
    [SerializeField] private float slipLimit = 0.2f;
    [SerializeField] private float penaltyTime = 1f;

    private float CurrentRotation { get; set; }
    public NetworkVariable<float> InputAcceleration = new NetworkVariable<float>();
    public NetworkVariable<float> InputSteering = new NetworkVariable<float>();
    public NetworkVariable<float> InputBrake = new NetworkVariable<float>();

    private Rigidbody _rigidbody;
    private float _steerHelper = 0.8f;

    private bool isPenalized = false;
    public Image fadeImage;
    private GameObject lastRoadSegment;
    private RespawnInfo lastRespawnInfo;

    private CheckpointManager checkpointManager;
    private LapTimeController lapTimeController;

    public float _currentSpeed = 0;
    private bool validReset = false;

    public int id;

    public GameObject projectilePrefab;
    public Transform projectileSpawn;
    public float projectileSpeed;
    public float projectileLife;

    private float Speed
    {
        get => _currentSpeed;
        set
        {
            if (Math.Abs(_currentSpeed - value) < float.Epsilon) return;
            _currentSpeed = value;
            OnSpeedChangeEvent?.Invoke(_currentSpeed);
        }
    }

    public delegate void OnSpeedChangeDelegate(float newVal);

    public event OnSpeedChangeDelegate OnSpeedChangeEvent;

    #endregion Variables

    #region Unity Callbacks

    public void Awake()
    {
        id = GetComponentInParent<Player>().id;
        projectileLife = 5;
        projectileSpeed = 80;

        _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody == null)
        {
            Debug.LogError("No Rigidbody found in children of Car.");
        }
    }

    //private void Start()
    //{
    //    NetworkObject networkObject = GetComponent<NetworkObject>();
    //    if (networkObject != null)
    //    {
    //        Debug.Log("NetworkObject OwnerClientId: " + networkObject.OwnerClientId);
    //        Debug.Log("IsOwner: " + IsOwner);
    //        Debug.Log("IsLocalPlayer: " + IsLocalPlayer);
    //    }
    //    else
    //    {
    //        Debug.LogError("NetworkObject no encontrado.");
    //    }
    //}

    private void OnEnable()
    {
        Debug.Log("OnEnable llamado en " + gameObject.name);

        NetworkObject networkObject = GetComponentInParent<NetworkObject>();
        if (networkObject != null)
        {
            Debug.Log("NetworkObject encontrado en " + gameObject.name + " con NetworkObjectId: " + networkObject.NetworkObjectId);
        }
        else
        {
            Debug.LogError("NetworkObject no encontrado en el objeto padre de " + gameObject.name);
        }

        if (NetworkManager.Singleton.IsClient)
        {
            Debug.Log("Este es un cliente.");
        }
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Este es un servidor.");
        }

        OnGameStarted();
        StartCoroutine(ReattemptFindComponents());
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log("OnNetworkSpawn llamado.");

        //if (!enabled)
        //{
        //    enabled = true;
        //    Debug.Log("CarController activado en OnNetworkSpawn.");
        //}

        if (IsServer)
        {
            InputSteering.Value = 0f;
            InputAcceleration.Value = 0f;
            InputBrake.Value = 0f;
        }

        if (IsClient)
        {
            Debug.Log("Cliente spawneado con NetworkObjectId: " + NetworkObjectId);
        }
    }

    public void Update()
    {
        Speed = _rigidbody.velocity.magnitude;

        if (IsOwner && IsSpawned && NetworkManager.Singleton.IsClient)
        {
            float steeringInput = Mathf.Clamp(Input.GetAxis("Horizontal"), -1, 1);
            float accelerationInput = Mathf.Clamp(Input.GetAxis("Vertical"), -1, 1);
            float brakeInput = Input.GetKey(KeyCode.Space) ? 1 : 0;

            Debug.Log("Enviando inputs al servidor: " + NetworkManager.LocalClientId);
            SubmitInputsServerRpc(steeringInput, accelerationInput, brakeInput);
        }
        else
        {
            Debug.Log("No es propietario o no está spawneado o no es cliente: " + NetworkManager.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitInputsServerRpc(float steering, float acceleration, float brake)
    {
        Debug.Log("SubmitInputsServerRpc llamado en el servidor.");
        if (IsServer && IsSpawned)
        {
            Debug.Log("Servidor recibiendo inputs: " + NetworkManager.LocalClientId);
            InputSteering.Value = steering;
            InputAcceleration.Value = acceleration;
            InputBrake.Value = brake;
        }
        else
        {
            Debug.LogWarning("Intento de escritura de cliente detectado en SubmitInputsServerRpc: " + NetworkManager.LocalClientId);
        }
    }

    public void FixedUpdate()
    {
        if (!IsServer || !IsSpawned) return;

        float steering = maxSteeringAngle * InputSteering.Value;
        float acceleration = InputAcceleration.Value;
        float brake = InputBrake.Value;

        Debug.Log($"FixedUpdate - Steering: {steering}, Acceleration: {acceleration}, Brake: {brake}");

        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }

            if (axleInfo.motor)
            {
                if (acceleration > float.Epsilon)
                {
                    axleInfo.leftWheel.motorTorque = forwardMotorTorque * acceleration;
                    axleInfo.leftWheel.brakeTorque = 0;
                    axleInfo.rightWheel.motorTorque = forwardMotorTorque * acceleration;
                    axleInfo.rightWheel.brakeTorque = 0;
                }

                if (acceleration < -float.Epsilon)
                {
                    axleInfo.leftWheel.motorTorque = backwardMotorTorque * acceleration;
                    axleInfo.leftWheel.brakeTorque = 0;
                    axleInfo.rightWheel.motorTorque = backwardMotorTorque * acceleration;
                    axleInfo.rightWheel.brakeTorque = 0;
                }

                if (Math.Abs(acceleration) < float.Epsilon)
                {
                    axleInfo.leftWheel.motorTorque = 0;
                    axleInfo.leftWheel.brakeTorque = engineBrake;
                    axleInfo.rightWheel.motorTorque = 0;
                    axleInfo.rightWheel.brakeTorque = engineBrake;
                }

                if (brake > 0)
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

    private IEnumerator ReattemptFindComponents()
    {
        while (checkpointManager == null || lapTimeController == null)
        {
            Debug.Log("Reintentando encontrar CheckpointManager y LapTimeController...");
            FindRequiredComponents();
            yield return new WaitForSeconds(1f);
        }
    }

    private void OnGameStarted()
    {
        if (_rigidbody == null)
        {
            _rigidbody = GetComponentInChildren<Rigidbody>();
            if (_rigidbody == null)
            {
                Debug.LogError("No Rigidbody found in children of Car.");
            }
        }

        FindRequiredComponents();
    }

    private void FindRequiredComponents()
    {
        checkpointManager = FindObjectOfType<CheckpointManager>();
        if (checkpointManager == null)
        {
            Debug.LogError("CheckpointManager not found in the scene.");
        }
        else
        {
            Debug.Log("CheckpointManager found.");
        }

        lapTimeController = FindObjectOfType<LapTimeController>();
        if (lapTimeController == null)
        {
            Debug.LogError("LapTimeController not found in the scene.");
        }
        else
        {
            Debug.Log("LapTimeController found.");
        }
    }

    public void SetLastRespawnInfo(RespawnInfo respawnInfo)
    {
        lastRespawnInfo = respawnInfo;
    }

    public void SetLastRoadSegment(GameObject roadSegment)
    {
        lastRoadSegment = roadSegment;
    }

    private bool IsCarInUnstablePosition()
    {
        float angleThreshold = 0.7f;

        if (Vector3.Dot(transform.up, Vector3.up) < -angleThreshold)
        {
            return true;
        }

        if (Mathf.Abs(Vector3.Dot(transform.right, Vector3.up)) > angleThreshold)
        {
            return true;
        }

        return false;
    }

    #region Colisiones y reaparición

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

        Checkpoint checkpoint = other.GetComponent<Checkpoint>();
        if (checkpoint != null && checkpointManager != null)
        {
            if (checkpoint.gameObject.name == "CheckPoint 0" && !validReset)
            {
                Debug.Log("No es valido, lo pongo true para la próxima");
                validReset = true;
            }
            checkpointManager.CheckpointReached(checkpoint);
        }

        if (other.CompareTag("Finish") && checkpointManager != null)
        {
            checkpointManager.FinishLineReached();
            if (!validReset)
            {
                // Si no es valido, NO hago nada                
            }
            if (validReset)
            {
                Debug.Log("Reinicio");
                lapTimeController.StartNewLap();
            }
        }
    }

    private IEnumerator ApplyPenalty()
    {
        isPenalized = true;
        yield return StartCoroutine(FadeToBlack());

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

    #region Methods

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
                    return; // wheels aren't on the ground so don't realign the rigidbody velocity
            }
        }

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
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawn.position, projectileSpawn.rotation);
        projectile.GetComponent<ProjectilCollision>().id = id;
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = projectileSpawn.forward * projectileSpeed;
        }
        Destroy(projectile, projectileLife);
        Debug.Log("Shooting from the car!");
    }

    #endregion
}
#endregion