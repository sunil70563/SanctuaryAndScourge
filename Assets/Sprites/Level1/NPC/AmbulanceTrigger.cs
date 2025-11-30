using UnityEngine;
using Unity.Netcode;

public class AmbulanceTrigger : MonoBehaviour
{
    // A reference to the "brain" on our parent object
    private AmbulanceController ambulanceController;

    void Start()
    {
        // Get the main controller from the parent
        ambulanceController = GetComponentInParent<AmbulanceController>();
    }

    // This now handles all trigger logic
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (ambulanceController == null) return;

        // Check if we hit a Civilian
        if (other.CompareTag("Civilian"))
        {
            if (other.TryGetComponent<CivilianState>(out CivilianState civilian))
            {
                // Tell the main controller to try and pick them up
                ambulanceController.TryToPickUp(civilian);
            }
        }

        // Check if we hit a Hospital
        if (other.CompareTag("Hospital"))
        {
            // Tell the main controller to try and deliver
            ambulanceController.TryToDeliver();
        }
    }
}