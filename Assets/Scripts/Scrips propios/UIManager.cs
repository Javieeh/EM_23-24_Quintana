using Unity.Netcode;
using UnityEngine;
using TMPro;

public class UIManager : NetworkBehaviour
{
    public TextMeshProUGUI pann;
    static TextMeshProUGUI pann2;
    private void Start()
    {
        pann2 = pann;
    }
    void OnGUI()
    {
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();
        }

        GUILayout.EndArea();
    }

    static void StartButtons()
    {
        if (GUILayout.Button("Host"))
        {
            pann2.text += "1";
            NetworkManager.Singleton.StartHost();
        }
        if (GUILayout.Button("Client"))
        {
            pann2.text += "1";
            NetworkManager.Singleton.StartClient();
        }
        if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
    }

    static void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ?
            "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +
            NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }
}
