using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class InputController : NetworkBehaviour
{
    private CarController car;

    private void Start()
    {
        car = GetComponentInParent<CarController>();
        if (car == null)
        {
            Debug.LogError("CarController no encontrado en el objeto padre.");
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            var input = context.ReadValue<Vector2>();
            Debug.Log($"Cliente OnMove - Acceleration: {input.y}, Steering: {input.x}");
            SubmitMoveInputServerRpc(input.y, input.x);
        }
    }

    public void OnBrake(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            var input = context.ReadValue<float>();
            Debug.Log($"Cliente OnBrake - Brake: {input}");
            SubmitBrakeInputServerRpc(input);
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (IsOwner && context.performed)
        {
            car.Shoot();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitMoveInputServerRpc(float acceleration, float steering)
    {
        Debug.Log($"Servidor SubmitMoveInputServerRpc - Acceleration: {acceleration}, Steering: {steering}");
        if (car != null && car.IsServer)
        {
            car.InputAcceleration.Value = acceleration;
            car.InputSteering.Value = steering;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitBrakeInputServerRpc(float brake)
    {
        Debug.Log($"Servidor SubmitBrakeInputServerRpc - Brake: {brake}");
        if (car != null && car.IsServer)
        {
            car.InputBrake.Value = brake;
        }
    }
}
