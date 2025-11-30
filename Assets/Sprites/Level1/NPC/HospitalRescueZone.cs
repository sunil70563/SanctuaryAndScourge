using UnityEngine;
using Unity.Netcode;

public class HospitalRescueZone : NetworkBehaviour
{
    [Header("Settings")]
    public string ambulanceTag = "Ambulance";
    public AudioSource audioSource;
    public AudioClip rescued;

    // Only the Server should handle game logic (score/rescuing)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        if (other.CompareTag(ambulanceTag))
        {
            // Get the controller from the ambulance object
            AmbulanceController ambulance = other.GetComponent<AmbulanceController>();

            // Check if the ambulance exists AND is actually carrying a civilian
            if (ambulance != null && ambulance.Status.Value == AmbulanceStatus.CarryingCivilian)
            {
                // 1. Perform the Rescue (Reset ambulance state)
                ambulance.TryToDeliver();

                // 2. Add Score for Player A
                // We reference the new MatchManager script here
                if (MatchManager.Instance != null)
                {
                    MatchManager.Instance.AddScorePlayerA(3);
                    Debug.Log("Rescue Successful! Score added to Player A.");
                    audioSource.PlayOneShot(rescued);
                }
                else
                {
                    Debug.LogWarning("MatchManager instance not found! Score not added.");
                }
            }
        }
    }
}