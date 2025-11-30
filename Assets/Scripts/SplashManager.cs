using UnityEngine;
using UnityEngine.SceneManagement; // Needed for loading scenes
using UnityEngine.Video; // Needed for the Video Player

[RequireComponent(typeof(VideoPlayer))] // Ensures a VideoPlayer is on this object
public class SplashManager : MonoBehaviour
{
    // Drag your main menu scene name here in the Inspector
    public string sceneToLoad; 

    private VideoPlayer videoPlayer;

    void Awake()
    {
        // Get the VideoPlayer component
        videoPlayer = GetComponent<VideoPlayer>();

        // Subscribe to the event that fires when the video is done
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    // This function is called when the video finishes
    void OnVideoFinished(VideoPlayer vp)
    {
        // Load your main scene
        SceneManager.LoadScene(sceneToLoad);
    }
}