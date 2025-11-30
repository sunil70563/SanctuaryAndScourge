using UnityEngine;

public class GameInfoTransfer : MonoBehaviour
{
    public static GameInfoTransfer Instance { get; private set; }

    [Header("Match Settings")]
    public float MatchDurationInMinutes = 3f; // Default

    private void Awake()
    {
        // Singleton pattern that survives scene loads
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetDuration(float minutes)
    {
        MatchDurationInMinutes = minutes;
    }
}