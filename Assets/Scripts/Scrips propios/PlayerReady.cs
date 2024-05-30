using Unity.Netcode;
using UnityEngine;

public class PlayerReady : NetworkBehaviour
{
    private NetworkVariable<bool> isReady = new NetworkVariable<bool>(false);

    private void Start()
    {
        
    }

    private void OnDestroy()
    {
        if (IsClient)
        {
            isReady.OnValueChanged -= OnReadyStatusChanged;
        }
    }

    private void OnReadyStatusChanged(bool oldStatus, bool newStatus)
    {
        
    }

    [ServerRpc]
    private void SetReadyServerRpc()
    {
        isReady.Value = true;
        Debug.Log(isReady.Value);

        PlayersManager.Instance.CheckReadyStatus();
    }

    public void SetReady()
    {
        if (IsOwner)
        {
            SetReadyServerRpc();
        }
    }

    public bool IsReady()
    {
        return isReady.Value;
    }
}
