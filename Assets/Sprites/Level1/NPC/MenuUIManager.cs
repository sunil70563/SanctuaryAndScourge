using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuUIManager : MonoBehaviour
{
    [Header("Main Menu References (Scene 1)")]
    public TMP_InputField joinCodeInput;
    public Button joinGameButton;
    [Tooltip("Text element to display errors (e.g. 'Wrong Code')")]
    public TMP_Text errorText; 

    [Header("Level Select References (Scene 2)")]
    public TMP_InputField durationInput;
    public Button startGameButton;

    void Start()
    {
        // Setup listeners if buttons exist
        if (joinGameButton != null)
            joinGameButton.onClick.AddListener(OnJoinGameClicked);

        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnCreateGameClicked);
            
        // Reset error text on start
        if (errorText != null)
            errorText.text = "";
    }

    // --- PLAYER A FLOW (Host/Create) ---
    private async void OnCreateGameClicked()
    {
        // 1. Save Duration Logic
        float minutes = 3; // Default
        if (durationInput != null && float.TryParse(durationInput.text, out float result))
        {
            minutes = result;
        }
        
        // Save to the GameInfoTransfer object so it persists to the game scene
        if (GameInfoTransfer.Instance != null)
        {
            GameInfoTransfer.Instance.SetDuration(minutes/60);
        }

        // 2. Create Relay
        if (RelayManager.Instance != null)
        {
            // Optional: Disable button to prevent double clicks while connecting
            if (startGameButton != null) startGameButton.interactable = false;
            
            // This waits for Unity Services, creates the room, 
            // and then RelayManager automatically loads the Game Scene.
            await RelayManager.Instance.CreateRelay();
        }
        else
        {
            Debug.LogError("RelayManager not found! Make sure the 'RelayManager' object exists in the Main Menu.");
        }
    }

    // --- PLAYER B FLOW (Client/Join) ---
    private async void OnJoinGameClicked()
    {
        // Clear previous error messages
        if (errorText != null) errorText.text = "";

        string code = joinCodeInput.text;
        
        // Basic validation: Is the code empty?
        if (string.IsNullOrEmpty(code)) 
        {
            if (errorText != null) errorText.text = "Please enter a code.";
            return;
        }

        if (RelayManager.Instance != null)
        {
            // Disable button and show status while connecting
            if (joinGameButton != null) joinGameButton.interactable = false;
            if (errorText != null) errorText.text = "Connecting...";

            // Attempt to join
            bool success = await RelayManager.Instance.JoinRelay(code);

            if (!success)
            {
                // FAILURE: Wrong code or connection error
                if (errorText != null) errorText.text = "Wrong code, try again.";
                
                // Re-enable button so they can try again
                if (joinGameButton != null) joinGameButton.interactable = true;
            }
            else
            {
                // SUCCESS: RelayManager is now loading the game scene automatically.
                // We don't need to do anything else.
                if (errorText != null) errorText.text = "Success!";
            }
        }
        else
        {
            Debug.LogError("RelayManager not found! Make sure the 'RelayManager' object exists in the Main Menu.");
            if (errorText != null) errorText.text = "System Error: Restart Game";
        }
    }
}