using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } 

    // --- Global Persistent Variables ---
    public string playerName = "GuestUser";
    public int musicVolume = 1;
    public int sfxVolume = 4;
    // ---------------------------------
    
    // Private variables to store default values (useful if defaults change)
    private readonly string defaultPlayerName = "GuestUser";
    private readonly int defaultMusicVolume = 3;
    private readonly int defaultSfxVolume = 3;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // New: Function to reset all persistent data
    public void ResetGameData()
    {
        Debug.Log("GameManager: Resetting all persistent game data.");
        
        // Resetting all public variables to their default values
        playerName = defaultPlayerName;
        musicVolume = defaultMusicVolume;
        sfxVolume = defaultSfxVolume;
        
        // You would add any other global variables here
        
        // Optional: Trigger an event if other systems need to know about the reset
        // EventSystem.Instance.TriggerGameDataReset(); 
    }
}