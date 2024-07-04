using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerName : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText; // Asigna un TextMesh para mostrar el nombre

    public NetworkVariable<NetworkString> playerName = new NetworkVariable<NetworkString>(new NetworkString { info = new Unity.Collections.FixedString32Bytes("Player") });

    private void Start()
    {
        if (IsClient)
        {
            playerName.OnValueChanged += OnNameChanged;
        }

        // Independientemente de si es el propietario o no, actualizamos el nombre.
        UpdateName();
    }

    private void OnDestroy()
    {
        if (IsClient)
        {
            playerName.OnValueChanged -= OnNameChanged;
        }
    }

    private void OnNameChanged(NetworkString oldName, NetworkString newName)
    {
        
        UpdateName();
    }

    [ServerRpc]
    private void SetNameServerRpc(NetworkString newName)
    {
        
        playerName.Value = newName;
    }

    public void SetName(string newName)
    {
        if (IsOwner)
        {
            
            SetNameServerRpc(new NetworkString { info = new Unity.Collections.FixedString32Bytes(newName) });
        }
    }

    public void SendCurrentNameToClient(ulong clientId)
    {
      
        SendCurrentNameClientRpc(clientId, playerName.Value);
    }

    [ClientRpc]
    public void SendCurrentNameClientRpc(ulong clientId, NetworkString currentName)
    {
       
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            playerName.Value = currentName;
            UpdateName();
        }
    }

    private void UpdateName()
    {
        if (nameText != null)
        {
            
            nameText.text = playerName.Value.ToString();
        }
    }
}
