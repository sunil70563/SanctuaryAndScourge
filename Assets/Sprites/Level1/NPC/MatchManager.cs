using UnityEngine;
using Unity.Netcode;

public class MatchManager : NetworkBehaviour
{
    public static MatchManager Instance { get; private set; }

    // --- Network Variables ---
    public NetworkVariable<float> TimeRemaining = new NetworkVariable<float>(
        180f, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> ScorePlayerA = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> ScorePlayerB = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );

    public NetworkVariable<bool> IsGameActive = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // --- UPDATED LOGIC HERE ---
            // Read from the persistent data carrier
            float durationInMinutes = 3f; // Default fallback
            
            if (GameInfoTransfer.Instance != null)
            {
                durationInMinutes = GameInfoTransfer.Instance.MatchDurationInMinutes;
            }

            // Convert to seconds
            TimeRemaining.Value = durationInMinutes * 60f;
            IsGameActive.Value = true;
        }
    }

    void Update()
    {
        if (!IsServer) return;

        if (IsGameActive.Value)
        {
            if (TimeRemaining.Value > 0)
            {
                TimeRemaining.Value -= Time.deltaTime;
            }
            else
            {
                TimeRemaining.Value = 0;
                EndGame();
            }
        }
    }

    private void EndGame()
    {
        IsGameActive.Value = false;
    }

    public void AddScorePlayerA(int amount) { if (IsServer && IsGameActive.Value) ScorePlayerA.Value += amount; }
    public void AddScorePlayerB(int amount) { if (IsServer && IsGameActive.Value) ScorePlayerB.Value += amount; }
}