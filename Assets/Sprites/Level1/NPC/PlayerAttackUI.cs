using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerAttackUI : NetworkBehaviour
{
    [Header("UI Reference")]
    [Tooltip("Drag your child 'AttackCanvas' object here")]
    public GameObject attackCanvas;

    [Header("Detection Settings")]
    [Tooltip("How close to a civilian to show the button")]
    public float detectionRadius = 3f;
    [Tooltip("Set this to your 'Civilians' layer")]
    public LayerMask civilianLayer;

    // --- Private ---
    private PlayerMovement playerMovement;
    private Button attackButton;

    // This runs for all players
    public override void OnNetworkSpawn()
    {
        // Get references
        playerMovement = GetComponent<PlayerMovement>();
        
        // Find the button on the child canvas
        if (attackCanvas != null)
        {
            attackButton = attackCanvas.GetComponentInChildren<Button>();
        }

        // We only want the owner to run this script
        if (!IsOwner)
        {
            this.enabled = false;
            if (attackCanvas != null) attackCanvas.SetActive(false); // Make sure it's hidden
            return;
        }

        // Add the listener for the button click
        if (attackButton != null)
        {
            attackButton.onClick.AddListener(OnAttackPressed);
        }
    }

    // --- THIS FUNCTION IS UPDATED ---
    // This runs every frame ONLY for the owner
    void Update()
    {
        // Default to hiding the button
        bool showButton = false;

        // Check in a circle around the player for any civilian
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, civilianLayer);

        if (hit != null)
        {
            // We hit *something* on the Civilian layer.
            // Now, we must check its STATE.
            if (hit.TryGetComponent<CivilianState>(out CivilianState civilian))
            {
                // We found the civilian's script.
                // ONLY show the button if this civilian is 'Healthy'.
                if (civilian.Status.Value == CivilianStatus.Healthy)
                {
                    showButton = true;
                }
                // If the status is 'Attacked', 'InAmbulance', 'Dead', or 'Saved',
                // 'showButton' will remain false.
            }
        }
        
        // Set the canvas active state based on our check
        if (attackCanvas != null)
        {
            attackCanvas.SetActive(showButton);
        }
    }

    // This function runs when the world-space button is clicked
    private void OnAttackPressed()
    {
        // Call the public 'TryAttack' function on our movement script
        if (playerMovement != null)
        {
            playerMovement.TryAttack();
        }
    }
}