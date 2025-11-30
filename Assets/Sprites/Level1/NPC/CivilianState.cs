using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;

// The possible states for a Civilian
public enum CivilianStatus
{
    Healthy,
    Attacked,
    InAmbulance, 
    Saved,
    Dead
}

public class CivilianState : NetworkBehaviour
{
    [Header("Settings")]
    public float timeToSave = 20f;

    [Header("UI References")]
    public Slider timerBar;

    [Header("Respawn Settings")]
    public LayerMask obstacleLayer;
    public Vector2 spawnAreaMin;
    public Vector2 spawnAreaMax;
    public float spawnCheckRadius = 0.5f;
    public int maxSpawnAttempts = 20;

    public AudioSource audiosource;
    public AudioClip died;

    // --- Network Variables ---
    public NetworkVariable<CivilianStatus> Status = new NetworkVariable<CivilianStatus>(
        CivilianStatus.Healthy,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<float> saveTimer = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    
    private SpriteRenderer spriteRenderer;
    private Collider2D civilianCollider; 

    // --- Setup ---
    public override void OnNetworkSpawn()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        civilianCollider = GetComponent<Collider2D>(); 

        // Subscribe to changes
        Status.OnValueChanged += OnStatusChanged;
        saveTimer.OnValueChanged += OnTimerChanged;

        // Immediately update visuals
        OnStatusChanged(CivilianStatus.Healthy, Status.Value);
        OnTimerChanged(0, saveTimer.Value);
    }

    public override void OnNetworkDespawn()
    {
        Status.OnValueChanged -= OnStatusChanged;
        saveTimer.OnValueChanged -= OnTimerChanged;
    }

    // --- Visual Updates (Run on ALL Clients) ---
    private void OnStatusChanged(CivilianStatus oldStatus, CivilianStatus newStatus)
    {
        switch (newStatus)
        {
            case CivilianStatus.Healthy:
                spriteRenderer.enabled = true;
                civilianCollider.enabled = true; 
                spriteRenderer.color = Color.white;
                timerBar.gameObject.SetActive(false);
                break;
            case CivilianStatus.Attacked:
                spriteRenderer.enabled = true;
                civilianCollider.enabled = true; 
                spriteRenderer.color = Color.yellow;
                timerBar.gameObject.SetActive(true);
                break;
            case CivilianStatus.InAmbulance:
                spriteRenderer.enabled = false;
                civilianCollider.enabled = false; 
                timerBar.gameObject.SetActive(false);
                break;
            case CivilianStatus.Saved:
                spriteRenderer.enabled = false;
                civilianCollider.enabled = false; 
                break;
            case CivilianStatus.Dead:
                spriteRenderer.enabled = true;
                civilianCollider.enabled = true; 
                spriteRenderer.color = Color.grey;
                timerBar.gameObject.SetActive(false);
                break;
        }
    }

    private void OnTimerChanged(float oldTime, float newTime)
    {
        timerBar.value = newTime / timeToSave;
    }

    // --- Server-Side Logic ---

    void Update()
    {
        if (!IsServer) return; // Server only

        if (Status.Value == CivilianStatus.Attacked)
        {
            saveTimer.Value -= Time.deltaTime;
            
            if (saveTimer.Value <= 0f)
            {
                Status.Value = CivilianStatus.Dead;

                // --- NEW: Add Score for Player B (The Infection/Attacker) ---
                if (MatchManager.Instance != null)
                {
                    MatchManager.Instance.AddScorePlayerB(1);
                    Debug.Log("Civilian Died. Score added to Player B.");
                    audiosource.PlayOneShot(died);
                }
                // ------------------------------------------------------------

                StartCoroutine(RespawnAfterDelay(2.0f));
            }
        }
    }

    private IEnumerator RespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Respawn();
    }

    private void Respawn()
    {
        if (!IsServer) return;

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
            TeleportAndResetClientRpc(spawnPos);
        }
        else
        {
            StartCoroutine(RespawnAfterDelay(5.0f));
        }
    }

    [ClientRpc]
    private void TeleportAndResetClientRpc(Vector2 newPosition)
    {
        transform.position = newPosition;

        if (IsServer)
        {
            Status.Value = CivilianStatus.Healthy;
        }
    }


    // --- Interaction Functions (Called by Players) ---

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void GetAttackedServerRpc()
    {
        if (Status.Value == CivilianStatus.Healthy)
        {
            Status.Value = CivilianStatus.Attacked;
            saveTimer.Value = timeToSave;
        }
    }
    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void GetPickedUpServerRpc(NetworkObjectReference ambulanceRef)
    {
        if (Status.Value != CivilianStatus.Attacked)
        {
            return; // Can't pick up unless attacked
        }

        if (ambulanceRef.TryGet(out NetworkObject ambulanceObject))
        {
            Status.Value = CivilianStatus.InAmbulance;
            float remainingTime = saveTimer.Value;
            saveTimer.Value = 0;
            
            ambulanceObject.GetComponent<AmbulanceController>().ReceiveCivilianTimer(remainingTime);
        }
    }
}