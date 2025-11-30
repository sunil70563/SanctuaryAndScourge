using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovement : NetworkBehaviour 
{
    [Header("Settings")]
    public float defaultSpeed = 4f;
    [Tooltip("Check this box for Player B (ENEMY).")]
    public bool invertFlipLogic = false;

    [Header("Player B Attack Settings")]
    public float attackRadius = 1.0f;
    public LayerMask civilianLayer; 

    [Header("Power Up Settings")]
    public float boostSpeed = 8f;
    public float slowedSpeed = 2f; // For Shock Drone
    public float boostDuration = 5f;
    public float freezeDuration = 3f;
    public float shieldDuration = 10f;
    public int maxPowerUpStack = 3;

    [Header("References")]
    public Rigidbody2D rb;
    public Animator anim;
    public SpriteRenderer spriteRenderer;
    public Collider2D playerCollider;
    public Transform visualsTransform; 
    
    public NetworkVariable<bool> IsDriving = new NetworkVariable<bool>(false);
    
    // Server Authoritative States
    private NetworkVariable<float> currentSpeed = new NetworkVariable<float>(4f);
    private NetworkVariable<bool> isFrozen = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> isShielded = new NetworkVariable<bool>(false); // NEW

    private float horizontal;
    private float vertical;
    private Dictionary<PowerUpType, int> powerUpInventory = new Dictionary<PowerUpType, int>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentSpeed.Value = defaultSpeed;
            isFrozen.Value = false;
            isShielded.Value = false;
        }

        if (IsOwner)
        {
            CameraController cam = Camera.main.GetComponent<CameraController>();
            if (cam != null) cam.SetTarget(transform);

            InitializeInventory();

            if (PowerUpUIManager.Instance != null)
            {
                // If invertFlipLogic is true, it's Player B (Destroyer)
                bool isPlayerA = !invertFlipLogic; 
                PowerUpUIManager.Instance.ShowPanelForPlayer(isPlayerA);
            }
        }
    }

    private void InitializeInventory()
    {
        // Add all types to inventory
        foreach(PowerUpType type in System.Enum.GetValues(typeof(PowerUpType)))
        {
            powerUpInventory[type] = 0;
            UpdateInventoryUI(type);
        }
    }

    void Update()
    {
        if (IsDriving.Value) return; 
        if (!IsOwner) return;

        if (isFrozen.Value)
        {
            UpdateMovementServerRpc(Vector2.zero);
            return;
        }

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        anim.SetFloat("Horizontal", Mathf.Abs(horizontal));
        anim.SetFloat("Vertical", Mathf.Abs(vertical));

        UpdateMovementServerRpc(new Vector2(horizontal, vertical));

        if (ShouldFlip(horizontal)) Flip();
    }
    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void UpdateMovementServerRpc(Vector2 inputVector)
    {
        if (IsDriving.Value) { rb.linearVelocity = Vector2.zero; return; }
        rb.linearVelocity = new Vector2(inputVector.x, inputVector.y) * currentSpeed.Value;
        
        if (ShouldFlip(inputVector.x)) Flip();
        anim.SetFloat("Horizontal", Mathf.Abs(inputVector.x));
        anim.SetFloat("Vertical", Mathf.Abs(inputVector.y));
    }

    // --- INVENTORY LOGIC ---

    [ClientRpc]
    public void GrantPowerUpClientRpc(PowerUpType type, ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner) return;

        if (powerUpInventory[type] < maxPowerUpStack)
        {
            powerUpInventory[type]++;
            UpdateInventoryUI(type);
        }
    }

    public void AttemptToActivatePowerUp(PowerUpType type)
    {
        if (!IsOwner) return;
        if (IsDriving.Value) return; 

        if (powerUpInventory.ContainsKey(type) && powerUpInventory[type] > 0)
        {
            powerUpInventory[type]--;
            UpdateInventoryUI(type);

            // --- ACTIVATE EFFECT ---
            switch (type)
            {
                // Player A
                case PowerUpType.SpeedBooster: RequestSpeedBoostServerRpc(); break;
                case PowerUpType.Shield:       RequestShieldServerRpc(); break;
                case PowerUpType.DroneMedic:   RequestMedicServerRpc(); break;
                case PowerUpType.HealthKit:    RequestHealthKitServerRpc(); break;

                // Player B
                case PowerUpType.Freezer:      RequestGlobalFreezeServerRpc(); break;
                case PowerUpType.ShockDrone:   RequestShockServerRpc(); break;
                case PowerUpType.Bomb:         RequestBombServerRpc(); break;
            }
        }
    }

    private void UpdateInventoryUI(PowerUpType type)
    {
        if (PowerUpUIManager.Instance != null)
        {
            PowerUpUIManager.Instance.UpdateCount(type, powerUpInventory[type], maxPowerUpStack);
        }
    }

    // --- POWER UP SERVER LOGIC ---

    // 1. SPEED (A)
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void RequestSpeedBoostServerRpc() { StartCoroutine(SpeedEffect(boostSpeed, boostDuration)); }

    // 2. SHIELD (A) - Blocks Freeze/Shock
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void RequestShieldServerRpc() { StartCoroutine(ShieldEffect()); }

    private IEnumerator ShieldEffect()
    {
        isShielded.Value = true;
        SetColorClientRpc(Color.green); // Visual feedback
        yield return new WaitForSeconds(shieldDuration);
        isShielded.Value = false;
        SetColorClientRpc(Color.white);
    }

    // 3. MEDIC (A) - Saves nearest Attacked Civilian
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void RequestMedicServerRpc()
    {
        CivilianState target = FindNearestCivilian(CivilianStatus.Attacked);
        if (target != null)
        {
            // Find our ambulance
            AmbulanceController amb = FindFirstObjectByType<AmbulanceController>();
            if (amb != null) target.GetPickedUpServerRpc(amb.NetworkObject);
        }
    }

    // 4. HEALTH KIT (A) - Resets Timer on ALL Attacked Civilians
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void RequestHealthKitServerRpc()
    {
        CivilianState[] civs = FindObjectsByType<CivilianState>(FindObjectsSortMode.None);
        foreach(var c in civs)
        {
            if (c.Status.Value == CivilianStatus.Attacked)
            {
                c.saveTimer.Value = c.timeToSave; // Reset timer
            }
        }
    }

    // 5. FREEZER (B) - Freezes Player A
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void RequestGlobalFreezeServerRpc()
    {
        ApplyEffectToEnemies(PowerUpType.Freezer);
    }

    // 6. SHOCK (B) - Slows Player A
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void RequestShockServerRpc()
    {
        ApplyEffectToEnemies(PowerUpType.ShockDrone);
    }

    // 7. BOMB (B) - Kills nearest Attacked Civilian
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void RequestBombServerRpc()
    {
        CivilianState target = FindNearestCivilian(CivilianStatus.Attacked);
        if (target != null)
        {
            target.saveTimer.Value = 0; // Instant kill
        }
    }

    // --- HELPER FUNCTIONS ---

    private void ApplyEffectToEnemies(PowerUpType type)
    {
        PlayerMovement[] allPlayers = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        foreach (var player in allPlayers)
        {
            if (player.OwnerClientId != OwnerClientId)
            {
                if (type == PowerUpType.Freezer) player.FreezeMeServerRpc();
                if (type == PowerUpType.ShockDrone) player.ShockMeServerRpc();
            }
        }
    }

    private CivilianState FindNearestCivilian(CivilianStatus requiredStatus)
    {
        CivilianState[] civs = FindObjectsByType<CivilianState>(FindObjectsSortMode.None);
        CivilianState bestTarget = null;
        float closestDist = Mathf.Infinity;

        foreach(var c in civs)
        {
            if (c.Status.Value == requiredStatus)
            {
                float d = Vector2.Distance(transform.position, c.transform.position);
                if (d < closestDist)
                {
                    closestDist = d;
                    bestTarget = c;
                }
            }
        }
        return bestTarget;
    }

    // --- VICTIM RPCS (Running on the person being attacked) ---

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Server)]
    public void FreezeMeServerRpc() 
    { 
        if (!isShielded.Value) StartCoroutine(FreezeEffect()); 
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Server)]
    public void ShockMeServerRpc() 
    { 
        if (!isShielded.Value) StartCoroutine(SpeedEffect(slowedSpeed, freezeDuration)); 
    }

    private IEnumerator FreezeEffect()
    {
        isFrozen.Value = true;
        currentSpeed.Value = 0; 
        SetColorClientRpc(Color.cyan);
        yield return new WaitForSeconds(freezeDuration);
        isFrozen.Value = false;
        currentSpeed.Value = defaultSpeed;
        SetColorClientRpc(Color.white);
    }

    private IEnumerator SpeedEffect(float speed, float duration)
    {
        currentSpeed.Value = speed;
        yield return new WaitForSeconds(duration);
        currentSpeed.Value = defaultSpeed;
    }

    [ClientRpc]
    private void SetColorClientRpc(Color newColor) { spriteRenderer.color = newColor; }


    // --- EXISTING HELPER LOGIC (Flipping, Driving, Camera) ---
    // (Kept exactly as previous version)
    public void SetDrivingState(bool isDriving) { SetDrivingStateServerRpc(isDriving); }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void SetDrivingStateServerRpc(bool isDriving) { IsDriving.Value = isDriving; SetPlayerVisibilityClientRpc(!isDriving); }
    [ClientRpc]
    private void SetPlayerVisibilityClientRpc(bool isVisible) { spriteRenderer.enabled = isVisible; playerCollider.enabled = isVisible; }
    
    public void TryAttack()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackRadius, civilianLayer);
        if (hit != null) {
            if (hit.TryGetComponent<CivilianAudioController>(out CivilianAudioController civAudio)) civAudio.PlayDyingSound();
            if (hit.TryGetComponent<CivilianState>(out CivilianState civilian)) {
                NetworkObject civilianObject = civilian.GetComponent<NetworkObject>();
                RequestAttackServerRpc(civilianObject);
            }
        }
    }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void RequestAttackServerRpc(NetworkObjectReference civilianRef) { if (civilianRef.TryGet(out NetworkObject civilianObject)) civilianObject.GetComponent<CivilianState>().GetAttackedServerRpc(); }
    private bool ShouldFlip(float horizontalInput) { if (invertFlipLogic) return (horizontalInput > 0 && visualsTransform.localScale.x > 0) || (horizontalInput < 0 && visualsTransform.localScale.x < 0); else return (horizontalInput > 0 && visualsTransform.localScale.x < 0) || (horizontalInput < 0 && visualsTransform.localScale.x > 0); }
    void Flip() { visualsTransform.localScale = new Vector3(visualsTransform.localScale.x * -1, visualsTransform.localScale.y, visualsTransform.localScale.z); }
    public bool ToggleZoom() { if (!IsOwner) return false; CameraController cam = Camera.main.GetComponent<CameraController>(); return cam != null && cam.ToggleViewMode(); }
}