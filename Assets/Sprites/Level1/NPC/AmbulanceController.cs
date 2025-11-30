using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI; 
using System.Collections; 

public enum AmbulanceStatus
{
    Empty,
    CarryingCivilian
}

public class AmbulanceController : NetworkBehaviour
{
    [Header("UI References")]
    public Slider timerBar;

    [Header("Driving Settings")]
    public float driveSpeed = 8f;
    public float turnSpeed = 200f;

    [Header("References")]
    public Animator anim; 

    // --- Network Variables ---
    public NetworkVariable<AmbulanceStatus> Status = new NetworkVariable<AmbulanceStatus>(AmbulanceStatus.Empty, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> civilianTimer = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> IsBeingDriven = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> networkMoveX = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> networkMoveY = new NetworkVariable<float>(-1f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> networkIsMoving = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    private float civilianMaxTime;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer; 

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>(); 
        
        Status.OnValueChanged += OnStatusChanged;
        civilianTimer.OnValueChanged += OnTimerChanged;
        networkMoveX.OnValueChanged += OnAnimParamsChanged;
        networkMoveY.OnValueChanged += OnAnimParamsChanged;
        networkIsMoving.OnValueChanged += OnAnimParamsChanged;

        OnStatusChanged(AmbulanceStatus.Empty, Status.Value);
    }

    public override void OnNetworkDespawn()
    {
        Status.OnValueChanged -= OnStatusChanged;
        civilianTimer.OnValueChanged -= OnTimerChanged;
        networkMoveX.OnValueChanged -= OnAnimParamsChanged;
        networkMoveY.OnValueChanged -= OnAnimParamsChanged;
        networkIsMoving.OnValueChanged -= OnAnimParamsChanged;
    }

    // --- Visual Update Functions ---
    private void OnAnimParamsChanged(float oldVal, float newVal) { UpdateAnimator(); }
    private void OnAnimParamsChanged(bool oldVal, bool newVal) { UpdateAnimator(); }
    private void UpdateAnimator()
    {
        anim.SetBool("IsMoving", networkIsMoving.Value);
        anim.SetFloat("MoveX", networkMoveX.Value);
        anim.SetFloat("MoveY", networkMoveY.Value);
    }

    private void OnStatusChanged(AmbulanceStatus oldStatus, AmbulanceStatus newStatus)
    {
        if (newStatus == AmbulanceStatus.Empty) timerBar.gameObject.SetActive(false);
        else if (newStatus == AmbulanceStatus.CarryingCivilian) timerBar.gameObject.SetActive(true);
    }

    private void OnTimerChanged(float oldTime, float newTime)
    {
        if (civilianMaxTime > 0) timerBar.value = newTime / civilianMaxTime;
    }

    // --- Server-Side Logic & Driving ---
    void Update()
    {
        if (IsServer)
        {
            if (Status.Value == AmbulanceStatus.CarryingCivilian)
            {
                civilianTimer.Value -= Time.deltaTime;
                if (civilianTimer.Value <= 0)
                {
                    Status.Value = AmbulanceStatus.Empty;
                    civilianTimer.Value = 0;
                }
            }
        }

        if (IsOwner)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            UpdateAmbulanceMovementServerRpc(horizontal, vertical);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void UpdateAmbulanceMovementServerRpc(float horizontal, float vertical)
    {
        if (!IsBeingDriven.Value) 
        {
            rb.linearVelocity = Vector2.zero;
            networkIsMoving.Value = false;
            return;
        }
        
        Vector2 moveInput = new Vector2(horizontal, vertical).normalized;
        bool isMoving = moveInput.magnitude > 0.1f;
        
        rb.linearVelocity = moveInput * driveSpeed;
        
        networkIsMoving.Value = isMoving;
        if (isMoving)
        {
            networkMoveX.Value = moveInput.x;
            networkMoveY.Value = moveInput.y;
        }
    }
    
    // --- PICKUP LOGIC ---
    public void TryToPickUp(CivilianState civilian)
    {
        if (!IsServer) return; 

        // If we are empty AND the civilian is attacked
        if (Status.Value == AmbulanceStatus.Empty && civilian.Status.Value == CivilianStatus.Attacked)
        {
            // Try to pick them up
            civilian.GetPickedUpServerRpc(NetworkObject);
        }
    }

    // --- UPDATED DELIVERY LOGIC (No Respawn) ---
    public void TryToDeliver()
    {
        if (!IsServer) return; 

        // If we are carrying a civilian
        if (Status.Value == AmbulanceStatus.CarryingCivilian)
        {
            // 1. Clear status
            Status.Value = AmbulanceStatus.Empty;
            civilianTimer.Value = 0;

            // 2. Log Success (Add Score logic here if needed)
            Debug.Log("Patient Delivered! Ambulance and Player remain at Hospital.");

            // 3. NO RESPAWN. We deleted the StartCoroutine(DelayedRespawn) line.
        }
    }

    // --- Public Functions ---
    public void ReceiveCivilianTimer(float remainingTime)
    {
        if (!IsServer) return; 
        Status.Value = AmbulanceStatus.CarryingCivilian;
        civilianTimer.Value = remainingTime;
        civilianMaxTime = remainingTime;
    }

    public void EnterVehicle(ulong clientId)
    {
        if (IsBeingDriven.Value) return; 
        IsBeingDriven.Value = true;
        NetworkObject.ChangeOwnership(clientId);
    }

    public void ExitVehicle()
    {
        IsBeingDriven.Value = false;
        NetworkObject.ChangeOwnership(0);
        rb.linearVelocity = Vector2.zero;
        networkIsMoving.Value = false;
    }

    // (Helper for visibility if needed later, but not used for respawning anymore)
    [ClientRpc]
    private void SetVisibilityClientRpc(bool isVisible)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = isVisible;
        }
        timerBar.transform.parent.gameObject.SetActive(isVisible);
    }
}