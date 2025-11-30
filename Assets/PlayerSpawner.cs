using UnityEngine;
using Unity.Netcode;

public class PlayerSpawner : MonoBehaviour 
{
    [Header("Player Prefabs")]
    public GameObject playerAPrefab; // Your PLAYER prefab
    public GameObject playerBPrefab; // Your ENEMY prefab

    private bool hasSpawnedHost = false;

    void Start()
    {
        if (NetworkManager.Singleton == null) return;

        // --- FIX: CHECK IF SERVER IS ALREADY RUNNING ---
        if (NetworkManager.Singleton.IsServer)
        {
            // The server started before this script was ready. 
            // Run the setup logic immediately.
            HandleServerStarted();
        }
        else
        {
            // The server hasn't started yet. Wait for the event.
            NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        }
    }

    private void HandleServerStarted()
    {
        if (NetworkManager.Singleton == null) return;

        // Subscribe to future connections (for Player B)
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        
        // --- Special Case: Handle the Host (Player A) ---
        // If we are the host, we are already connected (Client 0).
        // We spawn immediately.
        if (NetworkManager.Singleton.IsHost && !hasSpawnedHost)
        {
            SpawnPlayer(NetworkManager.Singleton.LocalClientId); 
            hasSpawnedHost = true;
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        // This runs when Player B connects
        // We verify it's not the host (just in case)
        if (clientId != NetworkManager.Singleton.LocalClientId) 
        {
            SpawnPlayer(clientId);
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        GameObject prefabToSpawn;

        // ID 0 is always the Host (Player A)
        if (clientId == 0) 
        {
            prefabToSpawn = playerAPrefab;
        }
        else // ID 1+ is Player B
        {
            prefabToSpawn = playerBPrefab;
        }

        if (prefabToSpawn != null)
        {
            GameObject playerInstance = Instantiate(prefabToSpawn);
            
            // IMPORTANT: Make sure the Z position is correct for 2D
            playerInstance.transform.position = new Vector3(0, 0, -1f); 

            // Spawn on network
            playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
            Debug.Log($"Spawned Player for ClientID: {clientId}");
        }
        else
        {
            Debug.LogError("Player Prefab missing in PlayerSpawner!");
        }
    }

    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        }
    }
}