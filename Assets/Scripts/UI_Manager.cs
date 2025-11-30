using UnityEngine;
using TMPro; // Required for TextMeshPro
using UnityEngine.UI; // Required for Button

[RequireComponent(typeof(AudioSource))] // Ensures AudioSource exists
public class UI_VisualManager : MonoBehaviour
{
    public AudioClip PanelOpeningClip;
    public AudioClip PanelClosingClip;
    private AudioSource audioSource;

    public TextMeshProUGUI greetingText;

    void Start()
    {
        UpdateGreetingText();

        // GetComponent is guaranteed to work because of [RequireComponent]
        audioSource = GetComponent<AudioSource>();

        // Find all buttons, even inactive ones
        Button[] allButtons = GetComponentsInChildren<Button>(true);

        foreach (Button button in allButtons)
        {
            if (button.GetComponent<ButtonScaleEffect>() == null)
            {
                button.gameObject.AddComponent<ButtonScaleEffect>();
            }
        }
    }

    public void PanelOpeningSfx()
    {
        // Calculate volume first
        float volume = GetCurrentSfxVolume();
        
        // Play the clip one time with the specified volume scale
        // This is much faster and won't be delayed.
        audioSource.PlayOneShot(PanelOpeningClip, volume);
    }

    public void PanelClosingSfx()
    {
        // Calculate volume first
        float volume = GetCurrentSfxVolume();

        // Play the clip one time with the specified volume scale
        audioSource.PlayOneShot(PanelClosingClip, volume);
    }

    /// <summary>
    /// Helper function to safely get the volume.
    /// </summary>
    private float GetCurrentSfxVolume()
    {
        if (GameManager.Instance != null)
        {
            // Note: PlayOneShot's volume is a scale (0-1).
            // Make sure sfxVolume / 5.0f results in a value like 1.0 or 0.8, etc.
            return GameManager.Instance.sfxVolume / 5.0f;
        }
        else
        {
            Debug.LogWarning("GameManager.Instance not found for sfxVolume, playing with default volume.");
            return 1.0f; // Fallback default
        }
    }

    public void UpdateGreetingText()
    {
        if (greetingText != null)
        {
            // Added a null check for GameManager.Instance to prevent errors on start
            if (GameManager.Instance != null)
            {
                greetingText.text = $"WELCOME BACK, {GameManager.Instance.playerName}!";
            }
            else
            {
                greetingText.text = "WELCOME BACK!"; // A safe fallback
            }
        }
    }
}