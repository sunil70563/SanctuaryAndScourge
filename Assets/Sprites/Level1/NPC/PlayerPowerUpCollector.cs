using UnityEngine;
using Unity.Netcode;

public class PlayerPowerUpCollector : NetworkBehaviour
{
    // This runs when the player touches any trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only the owner of this player can pick things up
        if (!IsOwner) return;

        // Check if the object we touched has the "PowerUpPickup" script
        if (other.TryGetComponent<PowerUpPickup>(out PowerUpPickup powerUp))
        {
            NetworkObject powerUpObject = other.GetComponent<NetworkObject>();

            // --- THIS IS THE FIX ---
            // We must check if the object we hit is fully spawned
            // on our local client. If it's not, we ignore it.
            // This prevents the race condition error.
            if (!powerUpObject.IsSpawned)
            {
                return;
            }
            // --- END OF FIX ---

            // It's a power-up and it's spawned! Ask the server to collect it.
            RequestPickupServerRpc(powerUpObject);
        }
    }

    // This is CALLED by the client, but RUNS on the SERVER
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void RequestPickupServerRpc(NetworkObjectReference powerUpRef)
    {
        // Find the power-up object on the server
        if (powerUpRef.TryGet(out NetworkObject powerUpObject))
        {
            // Tell the power-up to run its "Collect" logic
            powerUpObject.GetComponent<PowerUpPickup>().Collect(OwnerClientId);
        }
    }
}