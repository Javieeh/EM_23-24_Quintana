using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerCamera : NetworkBehaviour
{
    private void Start()
    {
        Debug.Log("PlayerCamera de " + this.gameObject.name + " iniciada...");
        if (IsOwner)
        {
            //Aqui da error object refenece, no esta bien instanciada la camara
            Camera.main.GetComponent<CinemachineVirtualCamera>().LookAt = this.transform;
            Camera.main.GetComponent<CinemachineVirtualCamera>().Follow = this.transform;
        }
    }
}
