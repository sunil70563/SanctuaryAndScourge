using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    private AudioSource audioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        UpdateVolume();
        audioSource.Play();
    }

    // Update is called once per frame
    public void UpdateVolume()
    {
        if (audioSource != null)
        {
            audioSource.volume = GameManager.Instance.musicVolume / 5.0f;
        }
    }
}
