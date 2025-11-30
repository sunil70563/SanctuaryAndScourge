using UnityEngine;
using Unity.Netcode;

// Defines all possible power-ups
public enum PowerUpType
{
    // Player A
    HealthKit,
    SpeedBooster,
    DroneMedic,
    Shield,
    
    // Player B
    ShockDrone,
    Bomb,
    Freezer
}

// Defines who can pick it up
public enum PowerUpAllegiance
{
    ForPlayerA,
    ForPlayerB
}

public class PowerUpPickup : NetworkBehaviour
{
    [Header("Power-Up Type")]
    [Tooltip("What kind of power-up is this?")]
    public PowerUpType type;

    [Tooltip("Who is allowed to pick this up?")]
    public PowerUpAllegiance allegiance;
    
    private bool isCollected = false;

    // This function runs ON THE SERVER
    public void Collect(ulong collectingPlayerId)
    {
        if (isCollected || !IsServer) return; // Already picked up or not on server

        // --- PERMISSION CHECK ---
        if (allegiance == PowerUpAllegiance.ForPlayerA && collectingPlayerId != 0)
        {
            return; // Player B tried to steal Player A's item.
        }
        if (allegiance == PowerUpAllegiance.ForPlayerB && collectingPlayerId != 1)
        {
            return; // Player A tried to steal Player B's item.
        }
        
        isCollected = true;
        
        // --- THIS IS THE UPDATED LOGIC ---
        // Find the correct spawner and tell it to decrement its count.
        if (allegiance == PowerUpAllegiance.ForPlayerA)
        {
            // Use FindFirstObjectByType (the new, faster method)
            FindFirstObjectByType<PlayerAPowerUpSpawner>().DecrementCount();
        }
        else if (allegiance == PowerUpAllegiance.ForPlayerB)
        {
            // Use FindFirstObjectByType (the new, faster method)
            FindFirstObjectByType<PlayerBPowerUpSpawner>().DecrementCount();
        }
        // --- END UPDATED LOGIC ---

        // Find the player who collected this
        NetworkObject playerObject = NetworkManager.Singleton.SpawnManager
            .GetPlayerNetworkObject(collectingPlayerId);
            
        // Create parameters to send RPC only to the collecting client
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { collectingPlayerId }
            }
        };

        // Call the 'GrantPowerUp' function on that specific player's client
        playerObject.GetComponent<PlayerMovement>()
            .GrantPowerUpClientRpc(type, clientRpcParams);
        
        // Destroy this power-up
        GetComponent<NetworkObject>().Despawn();
    }
}