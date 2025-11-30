using UnityEngine;
using Unity.Netcode;

// This script forces the NetworkManager to shut down when
// you stop Play Mode in the Editor or close the application.
public class NetworkManagerShutdown : MonoBehaviour
{
    // This is called when you stop Play Mode in the Editor
    // and when you close a built game.
    void OnApplicationQuit()
    {
        // Check if the NetworkManager exists
        if (NetworkManager.Singleton != null)
        {
            // Shut it down
            NetworkManager.Singleton.Shutdown();
        }
    }
}