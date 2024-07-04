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

        if (IsOwner)
        {
            SetName("Player" + NetworkManager.Singleton.LocalClientId); // Establece un nombre inicial

        }
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
        if (nameText != null)
        {
            nameText.text = newName.ToString();
        }
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
}
