using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    [Header("HUD References")]
    public TMP_Text timerText;
    public TMP_Text scoreAText;
    public TMP_Text scoreBText;

    [Header("Game Over Window")]
    public GameObject gameOverPanel;
    public TMP_Text winnerText;
    public TMP_Text finalScoreText;
    public Button homeButton;

    [Header("Audio Settings")]
    [Tooltip("Drag the AudioSource component from this Canvas here")]
    public AudioSource uiAudioSource;
    [Tooltip("The sound to play (tick/beep) when timer is red")]
    public AudioClip timerWarningClip;

    [Header("Scene Management")]
    [Tooltip("Name of your Main Menu scene")]
    public string menuSceneName = "MainMenu";

    // --- HARDCODED TEAM NAMES ---
    private string playerAName = "SAVIORS";
    private string playerBName = "DESTROYERS";

    public AudioSource audiosource;
    public AudioClip gameOver;

    private bool isGameOverShown = false;
    
    // Tracks the last second we played a sound for so we don't spam it
    private int lastSecondPlayed = -1; 
    float finalVolume = 1f;

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (homeButton != null)
            homeButton.onClick.AddListener(OnHomeClicked);
    }

    void Update()
    {
        if (MatchManager.Instance == null) return;

        // 1. Update Timer
        float time = MatchManager.Instance.TimeRemaining.Value;
        UpdateTimerDisplay(time);

        // 2. Update HUD Scores
        if (scoreAText != null) 
            scoreAText.text = $"{MatchManager.Instance.ScorePlayerA.Value}";
        
        if (scoreBText != null) 
            scoreBText.text = $"{MatchManager.Instance.ScorePlayerB.Value}";

        // 3. Check for Game Over
        if (!MatchManager.Instance.IsGameActive.Value && MatchManager.Instance.TimeRemaining.Value <= 0 && !isGameOverShown)
        {
            ShowGameOver();
        }
    }

    void UpdateTimerDisplay(float timeToDisplay)
    {
        if (timerText == null) return;

        // We add 1 for display purposes so it doesn't show 0:00 for a whole second
        float displayTime = timeToDisplay + 1; 
        
        float minutes = Mathf.FloorToInt(displayTime / 60);
        float seconds = Mathf.FloorToInt(displayTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        // --- COLOR & AUDIO LOGIC ---
        // Check if we are in the last 10 seconds (using the raw time passed in)
        
        if (timeToDisplay < 11 && timeToDisplay > 0)
        {
            // 1. Set Urgent Color
            timerText.color = new Color32(121, 14, 6, 255);

            // 2. Play Audio (Once per integer second)
            int currentIntSecond = Mathf.FloorToInt(displayTime);

            GameManager globalManager = FindFirstObjectByType<GameManager>();
            finalVolume *= globalManager.sfxVolume;

            if (currentIntSecond != lastSecondPlayed)
            {
                if (uiAudioSource != null && timerWarningClip != null)
                {
                    uiAudioSource.PlayOneShot(timerWarningClip, finalVolume);
                }
                lastSecondPlayed = currentIntSecond;
            }
        }
        else 
        {
            // Normal Color
            timerText.color = new Color32(149, 58, 52, 255);
            
            // Reset the tracker so it's ready for the next time it drops below 11
            lastSecondPlayed = -1; 
        }
    }

    void ShowGameOver()
    {
        audiosource.PlayOneShot(gameOver);
        isGameOverShown = true;
        gameOverPanel.SetActive(true);

        int scoreA = MatchManager.Instance.ScorePlayerA.Value;
        int scoreB = MatchManager.Instance.ScorePlayerB.Value;

        // --- WINNER LOGIC ---
        if (scoreA > scoreB)
        {
            winnerText.text = $"{playerAName}"; // "SAVIORS"
            winnerText.color = Color.green; 
        }
        else if (scoreB > scoreA)
        {
            winnerText.text = $"{playerBName}"; // "DESTROYERS"
            winnerText.color = Color.red; 
        }
        else
        {
            winnerText.text = "DRAW";
            winnerText.color = Color.white;
        }

        // Final Score Text
        finalScoreText.text = $"{scoreA} | {scoreB}";
    }

    void OnHomeClicked()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
        SceneManager.LoadScene(menuSceneName);
    }
}