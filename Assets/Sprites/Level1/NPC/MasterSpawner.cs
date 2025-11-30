using UnityEngine;
using Unity.Netcode;
using System.Collections; 

public class MasterSpawner : MonoBehaviour
{
    [Header("Spawners")]
    public CivilianSpawner civilianSpawner;
    public AmbulanceSpawner ambulanceSpawner;
    public PlayerAPowerUpSpawner playerAPowerUpSpawner;
    public PlayerBPowerUpSpawner playerBPowerUpSpawner;

    private bool hasStartedSpawning = false;

    void Start()
    {
        if (NetworkManager.Singleton == null) return;

        // --- FIX: CHECK IF SERVER IS ALREADY RUNNING ---
        if (NetworkManager.Singleton.IsServer)
        {
            OnServerReady();
        }
        else
        {
            NetworkManager.Singleton.OnServerStarted += OnServerReady;
        }
    }

    private void OnServerReady()
    {
        // Prevent double execution
        if (hasStartedSpawning) return;
        hasStartedSpawning = true;

        if (!NetworkManager.Singleton.IsServer) return;
        
        StartCoroutine(SpawnAllInOrder());
    }

    private IEnumerator SpawnAllInOrder()
    {
        Debug.Log("MasterSpawner: Starting Spawn Sequence...");

        // 1. Spawn Civilians
        if(civilianSpawner != null) civilianSpawner.SpawnAll();
        
        yield return new WaitForFixedUpdate();

        // 2. Spawn Ambulances
        if(ambulanceSpawner != null) ambulanceSpawner.SpawnAll();

        yield return new WaitForFixedUpdate();

        // 3. Start Powerups
        if(playerAPowerUpSpawner != null) playerAPowerUpSpawner.StartSpawning();
        if(playerBPowerUpSpawner != null) playerBPowerUpSpawner.StartSpawning();
        
        Debug.Log("MasterSpawner: Spawn Sequence Complete.");
    }

    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= OnServerReady;
        }
    }
}