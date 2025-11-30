using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SimpleObjectSound : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip hitSound;
    [Range(0f, 1f)] public float volume = 1f; // This is now a multiplier (Local * Master)


    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // Make sound 3D (louder when closer)
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
            PlaySound();
    }

    void PlaySound()
    {
        if (hitSound != null && audioSource != null)
        {
            float finalVolume = volume;

            // --- FIND GLOBAL VOLUME ---
            // Find the GameManager (The Main Menu one that persists)
            // Note: Ensure your Main Menu script is named 'GameManager' and not 'MatchManager'
            GameManager globalManager = FindFirstObjectByType<GameManager>();
            
            if (globalManager != null)
            {
                // Access the public variable for SFX Volume. 
                // ERROR CHECK: If your variable is named 'MasterSfxVolume' or 'SoundVolume', change 'sfxVolume' below.
                finalVolume *= globalManager.sfxVolume;
            }

            // PlayOneShot allows sound to play without cutting off previous sounds
            audioSource.PlayOneShot(hitSound, finalVolume);
        }
    }
}