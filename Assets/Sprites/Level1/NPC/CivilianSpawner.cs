// --- CivilianSpawner.cs (Updated) ---
using UnityEngine;
using Unity.Netcode;

public class CivilianSpawner : MonoBehaviour
{
    // (All your variables: civilianPrefabs, spawnAreaMin, obstacleLayer, etc. stay here)
    public GameObject[] civilianPrefabs;
    public Vector2 spawnAreaMin;
    public Vector2 spawnAreaMax;
    public LayerMask obstacleLayer;
    public float spawnCheckRadius = 0.5f;
    public int maxSpawnAttempts = 20;

    // RENAMED from SpawnCivilians and made PUBLIC
    public void SpawnAll()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        foreach (GameObject civilianPrefab in civilianPrefabs)
        {
            FindClearSpotAndSpawn(civilianPrefab);
        }
    }

    private void FindClearSpotAndSpawn(GameObject prefabToSpawn)
    {
        // (This function is unchanged)
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
            GameObject civilianInstance = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            civilianInstance.GetComponent<NetworkObject>().Spawn();
        }
        else
        {
            Debug.LogWarning($"Failed to find clear spawn position for {prefabToSpawn.name}");
        }
    }
}