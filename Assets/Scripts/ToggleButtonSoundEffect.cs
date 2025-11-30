using UnityEngine;
using UnityEngine.EventSystems; // Required for event interfaces

public class ToggleButtonSound : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Audio Clips")]
    [Tooltip("The sound to play when the button is turned ON")]
    [SerializeField] private AudioClip PressingSound; // New: Sound for the ON state
    [Tooltip("The sound to play when the button is turned OFF")]
    [SerializeField] private AudioClip ReleasingSound; // New: Sound for the OFF state
    
    private AudioSource audioSource;
    private bool isButtonPressed = false; // New: Tracks the current toggle state

    void Start()
    {
        // Make sure you have an AudioSource component attached to the GameObject
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("ToggleButtonSound requires an AudioSource component on the same GameObject.", this); // Added 'this' for context
        }
    }

    // Called when the pointer (mouse or touch) presses down on the button
    public void OnPointerDown(PointerEventData eventData)
    {        
        // 2. Play the sound based on the *new* state
        if (audioSource != null)
        {
            // Toggle the state BEFORE playing the sound so the sound matches the *result* of the click
            isButtonPressed = !isButtonPressed; 

            AudioClip clipToPlay = isButtonPressed ? PressingSound : ReleasingSound;

            if (clipToPlay != null)
            {
                // Assign and play the appropriate clip
                audioSource.clip = clipToPlay;
                // Added a null check for GameManager.Instance, though it should be a Singleton
                if (GameManager.Instance != null) 
                {
                    audioSource.volume = GameManager.Instance.sfxVolume / 5.0f;
                }
                else
                {
                    Debug.LogWarning("GameManager.Instance not found for sfxVolume, playing with default volume.");
                    audioSource.volume = 1.0f; // Fallback default
                }
                audioSource.Play(); 
            }
            else
            {
                // Play a simple sound if you want a fallback, or just log a warning
                Debug.LogWarning($"Toggle is {(isButtonPressed ? "ON" : "OFF")}, but the corresponding sound clip is missing.", this); // Added 'this' for context
                // Fallback to the original clip if you want a default press sound
                // audioSource.Play(); 
            }
        }
    }

    // Called when the pointer is released
    public void OnPointerUp(PointerEventData eventData)
    {
    }
}