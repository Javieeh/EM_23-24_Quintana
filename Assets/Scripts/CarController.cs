using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Netcode.Components;
using TMPro;

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
    public NetworkVariable<int> Position = new NetworkVariable<int>(); // Posicion respecto a los demas jugadores

    // Variables para comenzar sincronizados la carrera
    public NetworkVariable<bool> IsReady = new NetworkVariable<bool>(false);
    public NetworkVariable<int> CountdownValue = new NetworkVariable<int>(0);
    private TextMeshProUGUI countdownText;
    private bool raceStarted = false;
    
    private NetworkTransform _networkTransform;
    private NetworkRigidbody _networkRigidbody;
    private Rigidbody _rigidbody;
    private float _steerHelper = 0.8f;

    private bool isPenalized = false;
    public Image fadeImage;
    private GameObject lastRoadSegment;
    private RespawnInfo lastRespawnInfo;

    private CheckpointManager checkpointManager;
    private LapTimeController lapTimeController;

    public NetworkVariable<float> _currentSpeed = new NetworkVariable<float>();
    private bool validReset = false;

    public int id;
    private Vector3 lastRespawnPosition = Vector3.zero;
    private Quaternion lastRespawnRotation = Quaternion.identity;

    public GameObject projectilePrefab;
    public Transform projectileSpawn;
    public float projectileSpeed;
    public float projectileLife;

    public NetworkVariable<float> Speed
    {
        get => _currentSpeed;
        set
        {
            if (Math.Abs(_currentSpeed.Value - value.Value) < float.Epsilon) return;
            _currentSpeed = value;
            OnSpeedChangeEvent?.Invoke(_currentSpeed.Value);
        }
    }

    public delegate void OnSpeedChangeDelegate(float newVal);

    public event OnSpeedChangeDelegate OnSpeedChangeEvent;

    #endregion Variables

    #region Unity Callbacks

    public void Awake()
    {
        id = GetComponentInParent<Player>().ID.Value;
        projectileLife = 5;
        projectileSpeed = 80;
        projectileSpawn = transform.GetChild(6);
        projectilePrefab = projectilePrefab;

        _networkRigidbody = GetComponentInParent<NetworkRigidbody>();
        if (_networkRigidbody == null)
        {
            Debug.LogError("No Rigidbody found in children of Car.");
        }
        _networkRigidbody = GetComponent<NetworkRigidbody>();
        _networkTransform = GetComponent<NetworkTransform>(); // Asegurar obtener NetworkTransform

        if (_networkRigidbody == null)
        {
            Debug.LogError("No NetworkRigidbody found in Car.");
        }
        else
        {
            _rigidbody = _networkRigidbody.GetComponent<Rigidbody>();
            if (_rigidbody == null)
            {
                Debug.LogError("No Rigidbody found on the NetworkRigidbody.");
            }
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
        Speed.Value = _networkRigidbody.GetComponent<Rigidbody>().velocity.magnitude;
        if (IsOwner && IsCarInUnstablePosition() && !isPenalized && lastRespawnPosition != Vector3.zero)
        {
            ApplyPenaltyServerRpc(NetworkManager.Singleton.LocalClientId, lastRespawnPosition, lastRespawnRotation);
        }
        if (IsOwner && IsSpawned && NetworkManager.Singleton.IsClient)
        {
            float steeringInput = Mathf.Clamp(Input.GetAxis("Horizontal"), -1, 1);
            float accelerationInput = Mathf.Clamp(Input.GetAxis("Vertical"), -1, 1);
            float brakeInput = Input.GetKey(KeyCode.Space) ? 1 : 0;

            SubmitInputsServerRpc(steeringInput, accelerationInput, brakeInput);
        }
        else
        {
            //Debug.Log("No es propietario o no está spawneado o no es cliente: " + NetworkManager.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitInputsServerRpc(float steering, float acceleration, float brake)
    {
        if (IsServer && IsSpawned)
        {
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
        if (_networkRigidbody == null)
        {
            if (_networkRigidbody == null)
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
            //Debug.Log("CheckpointManager found.");
        }

        lapTimeController = FindObjectOfType<LapTimeController>();
        if (lapTimeController == null)
        {
            Debug.LogError("LapTimeController not found in the scene.");
        }
        else
        {
            //Debug.Log("LapTimeController found.");
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
        if (IsOwner && collision.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log($"Collision with Obstacle detected by {NetworkManager.Singleton.LocalClientId}");
            if (lastRespawnPosition != Vector3.zero)
            {
                ApplyPenaltyServerRpc(NetworkManager.Singleton.LocalClientId, lastRespawnPosition, lastRespawnRotation);
            }
            else
            {
                Debug.LogError("lastRespawnPosition is zero! Cannot respawn.");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsOwner)
        {
            if (other.CompareTag("Obstacle"))
            {
                Debug.Log($"Trigger with Obstacle detected by {NetworkManager.Singleton.LocalClientId}");
                if (lastRespawnPosition != Vector3.zero)
                {
                    ApplyPenaltyServerRpc(NetworkManager.Singleton.LocalClientId, lastRespawnPosition, lastRespawnRotation);
                }
                else
                {
                    Debug.LogError("lastRespawnPosition is zero! Cannot respawn.");
                }
            }

            if (other.CompareTag("Carretera"))
            {
                Debug.Log($"Trigger with Carretera detected by {NetworkManager.Singleton.LocalClientId}");
                RespawnInfo respawnInfo = other.GetComponent<RespawnInfo>();
                if (respawnInfo != null)
                {
                    SetLastRespawnInfo(respawnInfo.respawnPoint.position, respawnInfo.respawnPoint.rotation);
                    UpdateRespawnInfoServerRpc(NetworkManager.Singleton.LocalClientId, respawnInfo.respawnPoint.position, respawnInfo.respawnPoint.rotation);
                }
            }

            Checkpoint checkpoint = other.GetComponent<Checkpoint>();
            if (checkpoint != null && checkpointManager != null)
            {
                Debug.Log($"Checkpoint reached by {NetworkManager.Singleton.LocalClientId}");
                if (checkpoint.gameObject.name == "CheckPoint 0" && !validReset)
                {
                    validReset = true;
                }
            }

            if (other.CompareTag("Finish") && checkpointManager != null)
            {
                Debug.Log($"Finish line reached by {NetworkManager.Singleton.LocalClientId}");
                if (!validReset)
                {
                    // Si no es valido, NO hago nada                
                }
                if (validReset)
                {
                    lapTimeController.StartNewLap();
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RespawnCarServerRpc(Vector3 respawnPosition, Quaternion respawnRotation, ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out NetworkClient client))
        {
            var carController = client.PlayerObject.GetComponentInChildren<CarController>();
            if (carController != null)
            {
                carController._rigidbody.isKinematic = true; // Hacer cinemático temporalmente

                // Usar NetworkTransform para sincronizar
                carController._networkTransform.Teleport(respawnPosition, respawnRotation, carController.transform.localScale);

                carController._rigidbody.velocity = Vector3.zero;
                carController._rigidbody.angularVelocity = Vector3.zero;
                carController._rigidbody.isKinematic = false; // Desactivar cinemático

                Debug.Log($"New position: {carController._networkRigidbody.transform.position}, New rotation: {carController._networkRigidbody.transform.rotation}");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateRespawnInfoServerRpc(ulong clientId, Vector3 position, Quaternion rotation)
    {
        Debug.Log($"UpdateRespawnInfoServerRpc called by {clientId} with position {position} and rotation {rotation}");

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out NetworkClient client))
        {
            var carController = client.PlayerObject.GetComponentInChildren<CarController>();
            if (carController != null)
            {
                carController.SetLastRespawnInfo(position, rotation);
                Debug.Log($"Updated lastRespawnInfo for {clientId} with position {position} and rotation {rotation}");
            }
        }
        else
        {
            Debug.LogError($"Client {clientId} not found in ConnectedClients.");
        }
    }

    public void SetLastRespawnInfo(Vector3 position, Quaternion rotation)
    {
        lastRespawnPosition = position;
        lastRespawnRotation = rotation;
        Debug.Log($"Set lastRespawnInfo for {NetworkManager.Singleton.LocalClientId} with position {position} and rotation {rotation}");
    }

    [ServerRpc(RequireOwnership = false)]
    private void ApplyPenaltyServerRpc(ulong clientId, Vector3 position, Quaternion rotation)
    {
        Debug.Log($"ApplyPenaltyServerRpc called by {clientId}");
        StartCoroutine(ApplyPenaltyCoroutine(clientId, position, rotation));
    }

    private IEnumerator ApplyPenaltyCoroutine(ulong clientId, Vector3 respawnPosition, Quaternion respawnRotation)
    {
        isPenalized = true;
        Debug.Log($"Starting ApplyPenaltyCoroutine for {clientId}");

        // Enviar la información de reaparición solo al cliente que colisiona
        RespawnCarServerRpc(respawnPosition, respawnRotation, clientId);

        yield return new WaitForSeconds(penaltyTime);
        isPenalized = false;
    }

    #endregion

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
        float speed = _networkRigidbody.gameObject.GetComponent<Rigidbody>().velocity.magnitude;
        if (speed > topSpeed)
            _networkRigidbody.gameObject.GetComponent<Rigidbody>().velocity = topSpeed * _networkRigidbody.gameObject.GetComponent<Rigidbody>().velocity;
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
            _networkRigidbody.gameObject.GetComponent<Rigidbody>().velocity = velRotation * _networkRigidbody.gameObject.GetComponent<Rigidbody>().velocity;
        }

        CurrentRotation = transform.eulerAngles.y;
    }

    // Para empezar de forma sincronizada
    [ServerRpc]
    public void SetReadyServerRpc()
    {
        IsReady.Value = true;
    }

    [ClientRpc]
    public void StartRaceClientRpc()
    {
        raceStarted = true;
        // Habilitar controles del coche u otras acciones necesarias para iniciar la carrera
    }

    #endregion
    #region SHOOTING
    public void Shoot()
    {
        if (IsServer)
        {
            SpawnBullet();
        }
        else
        {
            SpawnBulletServerRpc();
        }
    }

    [ServerRpc]
    void SpawnBulletServerRpc()
    {
        SpawnBullet();
    }

    void SpawnBullet()
    {
        Debug.Log("Shooting from the car!");
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawn.position, projectileSpawn.rotation);
        projectile.GetComponent<ProjectilCollision>().id = id;
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = projectileSpawn.forward * projectileSpeed;
        }

        projectile.AddComponent<ProjectilCollision>();

        // Ensure the bullet has NetworkObject component and is spawned
        var networkObject = projectile.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();
        }

        //Destroy(projectile, projectileLife);

        StartCoroutine(DestroyBulletAfterTime(projectile, 2.0f));
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);

        var networkObject = bullet.GetComponent<NetworkObject>();
        if (networkObject != null && networkObject.IsSpawned)
        {
            networkObject.Despawn();
        }
    }
    [ClientRpc]
    public void UpdateCountdownClientRpc(int value)
    {
        CountdownValue.Value = value;
        if (countdownText != null)
        {
            countdownText.text = CountdownValue.Value > 0 ? CountdownValue.Value.ToString() : "GO!";
        }
    }


    #endregion
    #region RAFA

    /*
  [ServerRpc(RequireOwnership = false)]
      private void RespawnCarServerRpc(Vector3 respawnPosition, Quaternion respawnRotation, ulong clientId)
      {
          if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out NetworkClient client))
          {
              var carController = client.PlayerObject.GetComponentInChildren<CarController>();
              if (carController != null)
              {
                  carController._rigidbody.isKinematic = true; // Hacer cinemÃ¡tico temporalmente

                  // Usar NetworkTransform para sincronizar
                  carController._networkTransform.Teleport(respawnPosition, respawnRotation, carController.transform.localScale);

                  carController._rigidbody.velocity = Vector3.zero;
                  carController._rigidbody.angularVelocity = Vector3.zero;
                  carController._rigidbody.isKinematic = false; // Desactivar cinemÃ¡tico

                  Debug.Log($"New position: {carController._rigidbody.transform.position}, New rotation: {carController._rigidbody.transform.rotation}");
              }
          }
      }

      [ServerRpc(RequireOwnership = false)]
      private void UpdateRespawnInfoServerRpc(ulong clientId, Vector3 position, Quaternion rotation)
      {
          Debug.Log($"UpdateRespawnInfoServerRpc called by {clientId} with position {position} and rotation {rotation}");

          if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out NetworkClient client))
          {
              var carController = client.PlayerObject.GetComponentInChildren<CarController>();
              if (carController != null)
              {
                  carController.SetLastRespawnInfo(position, rotation);
                  Debug.Log($"Updated lastRespawnInfo for {clientId} with position {position} and rotation {rotation}");
              }
          }
          else
          {
              Debug.LogError($"Client {clientId} not found in ConnectedClients.");
          }
      }

      public void SetLastRespawnInfo(Vector3 position, Quaternion rotation)
      {
          lastRespawnPosition = position;
          lastRespawnRotation = rotation;
          Debug.Log($"Set lastRespawnInfo for {NetworkManager.Singleton.LocalClientId} with position {position} and rotation {rotation}");
      }

      [ServerRpc(RequireOwnership = false)]
      private void ApplyPenaltyServerRpc(ulong clientId, Vector3 position, Quaternion rotation)
      {
          Debug.Log($"ApplyPenaltyServerRpc called by {clientId}");
          StartCoroutine(ApplyPenaltyCoroutine(clientId, position, rotation));
      }

      private IEnumerator ApplyPenaltyCoroutine(ulong clientId, Vector3 respawnPosition, Quaternion respawnRotation)
      {
          isPenalized = true;
          Debug.Log($"Starting ApplyPenaltyCoroutine for {clientId}");

          // Enviar la informaciÃ³n de reapariciÃ³n solo al cliente que colisiona
          RespawnCarServerRpc(respawnPosition, respawnRotation, clientId);

          yield return new WaitForSeconds(penaltyTime);
          isPenalized = false;
      }*/

    #endregion
}