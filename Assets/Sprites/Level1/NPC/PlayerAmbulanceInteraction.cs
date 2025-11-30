using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro; 

[RequireComponent(typeof(PlayerMovement))]
public class PlayerAmbulanceInteraction : NetworkBehaviour
{
    [Header("UI References")]
    public GameObject interactCanvas;
    public Button interactButton;
    public TMP_Text interactText;

    [Header("Detection Settings")]
    public float detectionRadius = 3f;
    public LayerMask ambulanceLayer;

    private PlayerMovement playerMovement;
    private AmbulanceController currentAmbulance; 

    public override void OnNetworkSpawn()
    {
        playerMovement = GetComponent<PlayerMovement>();

        if (!IsOwner)
        {
            this.enabled = false;
            interactCanvas.SetActive(false);
            return;
        }

        interactButton.onClick.AddListener(OnInteractPressed);
    }

    void Update()
    {
        if (!IsOwner) return;

        // --- Logic for "GET OUT" button ---
        if (playerMovement.IsDriving.Value)
        {
            interactCanvas.SetActive(true);
            interactText.text = "Get Out";
            
            // Find the ambulance we are driving
            if (currentAmbulance == null)
            {
                var ambulances = FindObjectsByType<AmbulanceController>(FindObjectsSortMode.None);
                foreach (var amb in ambulances)
                {
                    if (amb.IsOwner) 
                    {
                        currentAmbulance = amb;
                        break;
                    }
                }
            }
            
            // --- FORCE POSITION UPDATE ---
            // If we are driving, force our transform to match the ambulance
            if (currentAmbulance != null)
            {
                transform.position = currentAmbulance.transform.position;
            }
        }
        // --- Logic for "GET IN" button ---
        else
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, ambulanceLayer);

            if (hit != null)
            {
                if (hit.TryGetComponent<AmbulanceController>(out AmbulanceController ambulance) || 
                    (hit.transform.parent != null && hit.transform.parent.TryGetComponent<AmbulanceController>(out ambulance)))
                {
                    if (!ambulance.IsBeingDriven.Value)
                    {
                        interactCanvas.SetActive(true);
                        interactText.text = "Get In";
                        currentAmbulance = ambulance;
                    }
                    else
                    {
                        interactCanvas.SetActive(false); 
                    }
                }
                else 
                {
                     interactCanvas.SetActive(false);
                     currentAmbulance = null;
                }
            }
            else
            {
                interactCanvas.SetActive(false);
                currentAmbulance = null;
            }
        }
    }

    private void OnInteractPressed()
    {
        if (currentAmbulance == null) return;

        if (playerMovement.IsDriving.Value)
        {
            RequestExitVehicleServerRpc(currentAmbulance.NetworkObject);
        }
        else if (currentAmbulance != null)
        {
            RequestEnterVehicleServerRpc(currentAmbulance.NetworkObject);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void RequestEnterVehicleServerRpc(NetworkObjectReference ambulanceRef)
    {
        if (ambulanceRef.TryGet(out NetworkObject ambulanceObject))
        {
            ambulanceObject.GetComponent<AmbulanceController>().EnterVehicle(OwnerClientId);
            playerMovement.SetDrivingState(true);

            // 1. Parent the player to the ambulance
            NetworkObject.TrySetParent(ambulanceObject);
            
            // 2. Reset local position so player is exactly "inside" the ambulance
            transform.localPosition = Vector3.zero;
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void RequestExitVehicleServerRpc(NetworkObjectReference ambulanceRef)
    {
        if (ambulanceRef.TryGet(out NetworkObject ambulanceObject))
        {
            ambulanceObject.GetComponent<AmbulanceController>().ExitVehicle();
            playerMovement.SetDrivingState(false);

            // 1. Un-parent from the ambulance
            NetworkObject.TrySetParent((NetworkObject)null);

            // 2. Move player to a safe exit spot relative to the ambulance
            // (e.g. 2 units to the right of the ambulance)
            transform.position = ambulanceObject.transform.position + (ambulanceObject.transform.right * 2.0f);
        }
    }
}