using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using Unity.Netcode;

public class ZoomUIController : MonoBehaviour
{
    [Tooltip("The single button that toggles zoom")]
    public Button toggleZoomButton;
    
    [Tooltip("The text component inside the button")]
    public TMP_Text buttonText; // Assign this in Inspector!

    void Start()
    {
        if (toggleZoomButton != null)
        {
            toggleZoomButton.onClick.AddListener(OnToggleZoomClicked);
        }
        
        // Set initial text to "X2" (Assuming we start in Follow Mode)
        if(buttonText != null) buttonText.text = "X2";
    }

    void OnToggleZoomClicked()
    {
        var players = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        
        foreach (var player in players)
        {
            if (player.IsOwner)
            {
                // Call toggle and get the new state
                bool isFullMap = player.ToggleZoom();
                
                // Update Text based on state
                if (buttonText != null)
                {
                    if (isFullMap)
                    {
                        // We are now in Full Map -> Button should say "X1" (Go back)
                        buttonText.text = "X1";
                    }
                    else
                    {
                        // We are now Following -> Button should say "X2" (Go to Map)
                        buttonText.text = "X2";
                    }
                }
                return; 
            }
        }
    }
}