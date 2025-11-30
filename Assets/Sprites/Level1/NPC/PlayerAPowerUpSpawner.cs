using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlayerAPowerUpSpawner : MonoBehaviour
{
    [Header("Power-Up Prefabs")]
    [Tooltip("Add all of Player A's power-up prefabs here")]
    public GameObject[] powerUpPrefabs;

    [Header("Spawn Timing")]
    public float spawnInterval = 15f;
    public int maxPowerUps = 5;
    
    [Header("Spawn Area")]
    public Vector2 spawnAreaMin;
    public Vector2 spawnAreaMax;

    [Header("Collision Settings")]
    public LayerMask obstacleLayer;
    public float spawnCheckRadius = 0.5f;
    public int maxSpawnAttempts = 20;
    
    private int currentPowerUpCount = 0;

    // This is called by MasterSpawner
    public void StartSpawning()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        
        // We no longer need to listen for events here.
        StartCoroutine(SpawnPowerUpRoutine());
    }

    // --- NEW FUNCTION ---
    // This is called by the PowerUpPickup script when it's collected.
    public void DecrementCount()
    {
        currentPowerUpCount--;
    }
    // --- END NEW FUNCTION ---

    // The OnDestroy() method is no longer needed.

    private IEnumerator SpawnPowerUpRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            if (currentPowerUpCount < maxPowerUps)
            {
                SpawnRandomPowerUp();
            }
        }
    }

    private void SpawnRandomPowerUp()
    {
        Vector2 spawnPos = Vector2.zero;
        bool positionFound = false;

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
            float y = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
            spawnPos = new Vector2(x, y);

            Collider2D hit = Physics2D.OverlapCircle(spawnPos, spawnCheckRadius, obstacleLayer);

            if (hit == null)
            {
                positionFound = true;
                break;
            }
        }

        if (positionFound)
        {
            GameObject prefabToSpawn = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];
            GameObject powerUpInstance = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            
            NetworkObject netObj = powerUpInstance.GetComponent<NetworkObject>();
            netObj.Spawn();
            
            currentPowerUpCount++;
        }
        else
        {
            Debug.LogWarning($"Failed to find clear spawn position for {gameObject.name}");
        }
    }
}