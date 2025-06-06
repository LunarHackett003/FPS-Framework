using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ConnectionUGUI : MonoBehaviour
{
    void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
    void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }
    void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }
    void Shutdown()
    {
        NetworkManager.Singleton.Shutdown();
    }
    private void OnGUI()
    {
        if(NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.ShutdownInProgress)
            {
                GUILayout.Label("Shutting Down...");
                return;
            }
            if(NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
            {
                if (GUILayout.Button("Disconnect"))
                    Shutdown();
            }
            else
            {
                if (GUILayout.Button("Start Server"))
                    StartServer();
                if (GUILayout.Button("Start Host"))
                    StartHost();
                if (GUILayout.Button("Start Client"))
                    StartClient();
            }
        }
        else
        {
            GUILayout.Label("No Network Manager!");
        }
    }
}
