using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CameraManager : Singleton<CameraManager>
{
    private CinemachineVirtualCamera virtualCamera;

    void Start()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    void Update()
    {
        if (NetworkManager.Singleton.LocalClient != null)
        {
            ulong localClientId = NetworkManager.Singleton.LocalClient.ClientId;
            if (PlayersManager.Instance.TryGetPlayer(localClientId, out GameObject localPlayer))
            {
                //SetPlayer(localPlayer.transform.GetChild(0));
            }
        }
    }

    public void SetPlayer(Transform playerTransform)
    {
        if (virtualCamera != null)
        {
            virtualCamera.LookAt = playerTransform;
            virtualCamera.Follow = playerTransform;
        }
    }
}
