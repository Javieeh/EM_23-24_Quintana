using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerCamera : NetworkBehaviour
{
    private void Start()
    {
        if (IsOwner)
        {
            CameraManager.Instance.SetPlayer(transform.GetChild(0)); // Asigna la cámara al jugador local
        }
    }
}
