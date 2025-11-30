using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CivilianAudioController : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip screamClip; // Enemy approaches (unharmed)
    public AudioClip dyingClip;  // Enemy clicks ATTACK button
    public AudioClip helpClip;   // Player A approaches (harmed)

    [Header("Tags")]
    public string enemyTag = "Enemy";
    public string playerTag = "Player";

    [Header("Settings")]
    public float soundCooldown = 2.0f; 

    // --- STATE ---
    private bool isHarmed = false; 
    private AudioSource audioSource;
    private float nextSoundTime = 0f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // --- LOGIC 1: PROXIMITY (Scream / Help) ---
    // Requires CircleCollider2D (Is Trigger = True)
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Enemy approaches & Civilian is Healthy -> SCREAM
        if (!isHarmed && other.CompareTag(enemyTag))
        {
            TryPlaySound(screamClip);
        }

        // 2. Player A approaches & Civilian is Harmed -> HELP ME
        if (isHarmed && other.CompareTag(playerTag))
        {
            TryPlaySound(helpClip);
        }
    }

    // --- LOGIC 2: PUBLIC FUNCTION FOR ATTACK ---
    // This must be called by your Player's Attack Script
    public void PlayDyingSound()
    {
        if (!isHarmed)
        {
            // Play sound immediately
            if (dyingClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(dyingClip);
            }

            // Change state to Harmed
            isHarmed = true;

            // Optional: Visual feedback (turn red)
            GetComponent<SpriteRenderer>().color = Color.red;
        }
    }

    // Helper for proximity sounds (includes cooldown)
    private void TryPlaySound(AudioClip clip)
    {
        if (Time.time >= nextSoundTime && clip != null)
        {
            audioSource.PlayOneShot(clip);
            nextSoundTime = Time.time + soundCooldown;
        }
    }
}