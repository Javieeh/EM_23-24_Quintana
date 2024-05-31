using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class InputController : NetworkBehaviour
{
    private CarController car;

    private void Start()
    {
        car = GetComponentInChildren<CarController>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            var input = context.ReadValue<Vector2>();
            car.InputAcceleration = input.y;
            car.InputSteering = input.x;
        }
    }

    public void OnBrake(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            var input = context.ReadValue<float>();
            car.InputBrake = input;
        }

    }

    public void OnAttack(InputAction.CallbackContext context)
    {
    }
}