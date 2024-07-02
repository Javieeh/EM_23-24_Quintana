using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerCamera : NetworkBehaviour
{
    private CinemachineVirtualCamera virtualCamera;
     

    private void Start()
    {
        virtualCamera = GameObject.FindGameObjectWithTag("VirtualCamera").GetComponent<CinemachineVirtualCamera>();
        Debug.Log("PlayerCamera de " + this.gameObject.name + " iniciada...");
        //Aqui da error object refenece, no esta bien instanciada la camara
        virtualCamera.LookAt = this.transform.GetChild(0);
        virtualCamera.Follow = this.transform.GetChild(0);

    }
}
