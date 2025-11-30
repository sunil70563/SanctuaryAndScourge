using UnityEngine;
using Unity.Netcode;

public class AmbulanceSpawner : MonoBehaviour
{
    [Header("Ambulance Prefabs")]
    [Tooltip("Add your 2 Ambulance prefabs here")]
    public GameObject[] ambulancePrefabs;

    [Header("Spawn Area")]
    public Vector2 spawnAreaMin;
    public Vector2 spawnAreaMax;

    [Header("Collision Settings")]
    public LayerMask obstacleLayer;
    public float spawnCheckRadius = 1.0f; // Ambulances are bigger
    public int maxSpawnAttempts = 20;

    // This is now called by MasterSpawner
    public void SpawnAll()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        foreach (GameObject ambulancePrefab in ambulancePrefabs)
        {
            FindClearSpotAndSpawn(ambulancePrefab);
        }
    }

    private void FindClearSpotAndSpawn(GameObject prefabToSpawn)
    {
        Vector2 spawnPos = Vector2.zero;
        bool positionFound = false;

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
            float y = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
            spawnPos = new Vector2(x, y);

            // Check if the spot is clear
            Collider2D hit = Physics2D.OverlapCircle(spawnPos, spawnCheckRadius, obstacleLayer);

            if (hit == null)
            {
                positionFound = true;
                break; // Found a clear spot
            }
        }

        if (positionFound)
        {
            // Spawn the ambulance
            GameObject ambulanceInstance = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            ambulanceInstance.GetComponent<NetworkObject>().Spawn();
        }
        else
        {
            Debug.LogWarning($"Failed to find clear spawn position for {prefabToSpawn.name}");
        }
    }
    
    // The Start() and OnDestroy() methods are removed
    // because MasterSpawner now controls this script.
}