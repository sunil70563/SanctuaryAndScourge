using UnityEngine;
using UnityEngine.UI; // Required for Slider
using TMPro; // Required if using TextMeshPro Input Field
using UnityEngine.SceneManagement;

public class OptionsPanelManager : MonoBehaviour
{
    // Drag your UI elements here in the Inspector
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Slider soundSlider;
    [SerializeField] private Slider sfxSlider;

    [SerializeField] private AudioClip previewSfx;

    private BackgroundMusicManager musicManager;
    public GameObject resetPanel;

    //Temperary Values Holders
    public string tempPlayerName;
    public int tempMusicVolume;
    public int tempSfxVolume;

    // We store the 'saved' music volume to revert to on cancel
    private int savedMusicVolume;

    // 'Awake' runs once when the object is first created
    void Awake()
    {
        musicManager = Object.FindFirstObjectByType<BackgroundMusicManager>();
        
        // Add listeners just once. 'Awake' is better than 'Start' for this.
        playerNameInput.onEndEdit.AddListener(OnPlayerNameChanged); 
        soundSlider.onValueChanged.AddListener(OnMusicVolumeChanged); 
        sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
    }

    // 'OnEnable' runs EVERY time the GameObject is set to active
    void OnEnable()
    {
        // This is the perfect place to load data
        
        // 1. Load saved data from GameManager into temp variables
        tempPlayerName = GameManager.Instance.playerName;
        tempMusicVolume = GameManager.Instance.musicVolume;
        tempSfxVolume = GameManager.Instance.sfxVolume;

        // 2. Store the currently saved music volume for "cancel"
        savedMusicVolume = GameManager.Instance.musicVolume;
        
        // 3. Update the UI to match the loaded data
        LoadSettings();
    }

    // Pulls data from temp variables and updates the UI
    public void LoadSettings()
    {
        if (playerNameInput == null || soundSlider == null || sfxSlider == null)
        {
            Debug.LogError("OptionsPanelManager is missing UI references!");
            return;
        }
        
        // Load data from the TEMPORARY variables into the UI
        playerNameInput.text = tempPlayerName;
        soundSlider.value = tempMusicVolume;
        sfxSlider.value = tempSfxVolume;
    }

    // --- These functions are called by the listeners ---

    public void OnPlayerNameChanged(string newName)
    {
        tempPlayerName = newName;
    }

    public void OnMusicVolumeChanged(float volume)
    {
        tempMusicVolume = (int)volume;
        
        // --- This is how you do a live preview ---
        // 1. Temporarily set the GameManager's volume
        GameManager.Instance.musicVolume = tempMusicVolume;
        
        // 2. Tell the music manager to update
        if (musicManager != null)
        {
            musicManager.UpdateVolume();
        }
        // If we "Cancel", we will set this back to its saved value
    }

    public void OnSfxVolumeChanged(float volume)
    {
        tempSfxVolume = (int)volume;
        
        // You could play a sample SFX here for preview
        float actualVolume = volume / 5.0f;
        // Play the sound at the camera's position
        if (previewSfx != null)
        {
            // We use Camera.main.transform.position so the sound
            // is 2D and not panned to one side.
            AudioSource.PlayClipAtPoint(previewSfx, Camera.main.transform.position, actualVolume);
        }
    }

    // --- These functions are called by your Buttons ---

    public void OnResetButtonPressed()
    {
        // 1. Reset the official data
        GameManager.Instance.ResetGameData();

        // 2. Update temp variables to match the new defaults
        tempPlayerName = GameManager.Instance.playerName;
        tempMusicVolume = GameManager.Instance.musicVolume;
        tempSfxVolume = GameManager.Instance.sfxVolume;

        // 3. Update the UI to show the new defaults
        LoadSettings();

        // 4. Update the live music preview
        if (musicManager != null)
        {
            musicManager.UpdateVolume();
        }
        CloseResetPanel();
    }

    // This is your "Save" or "Apply" button
    public void saveSettings()
    {
        // 1. Permanently save temp values to the GameManager
        GameManager.Instance.playerName = tempPlayerName;
        GameManager.Instance.musicVolume = tempMusicVolume;
        GameManager.Instance.sfxVolume = tempSfxVolume;

        // 2. Update the 'saved' volume to this new value
        savedMusicVolume = tempMusicVolume;
        
        // 3. Tell the music manager to update (it will read the new GM values)
        if (musicManager != null)
        {
            musicManager.UpdateVolume();
        }
        CloseOptionsPanel(); 
    }

    // This is your "Close" or "Cancel" button
    public void CloseOptionsPanel()
    {
        // 1. Revert the live preview by restoring the saved volume
        GameManager.Instance.musicVolume = savedMusicVolume;
        if (musicManager != null)
        {
            musicManager.UpdateVolume();
        }

        // 2. Disable the panel
        gameObject.SetActive(false); 
        
        // (No need to call LoadSettings() because OnEnable will
        // handle it next time the panel opens)
    }

    // --- Sub-Panel Management ---

    public void OpenOptionsPanel()
    {
        // This function just needs to enable the panel.
        // OnEnable() will handle the rest.
        gameObject.SetActive(true); 
    }

    public void OpenResetPanel()
    {
        if (resetPanel != null)
        {
            resetPanel.SetActive(true);
        } 
    }

    public void CloseResetPanel()
    {
        if (resetPanel != null)
        {
            resetPanel.SetActive(false);
        }
    }
}