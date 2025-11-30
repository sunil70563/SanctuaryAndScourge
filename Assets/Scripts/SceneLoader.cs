using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Required for Coroutines

public class SceneLoader : MonoBehaviour
{
    public AudioClip startSound;
    private AudioSource audioSource;

    void Start()
    {
        // Find or add an AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0;
        }
    }

    // --- THIS IS THE FUNCTION YOUR CITIES NEED ---
    // Loads a scene instantly.
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    
    // --- THIS IS THE FUNCTION YOUR START BUTTON NEEDS ---
    // Plays a sound, THEN loads a scene.
    public void LoadSceneWithSound(string sceneName)
    {
        StartCoroutine(PlaySoundAndLoad(sceneName));
    }

    IEnumerator PlaySoundAndLoad(string sceneName)
    {
        // Play the sound
        if (audioSource != null && startSound != null)
        {
            audioSource.PlayOneShot(startSound);
            // Wait for the sound to finish
            yield return new WaitForSeconds(startSound.length);
        }

        // Now, load the scene
        SceneManager.LoadScene(sceneName);
    }
}