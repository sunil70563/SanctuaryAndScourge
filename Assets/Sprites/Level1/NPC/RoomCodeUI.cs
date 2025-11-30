using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;

public class RoomCodeUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject codePanel;
    public TMP_InputField codeDisplayField; // Used for display and copying
    public Button closeButton;

    void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
        
        // Wait slightly for connection to settle
        Invoke("SetupCodeDisplay", 0.5f);
    }

    void SetupCodeDisplay()
    {
        // Only show this panel if we are the HOST (Player A)
        // Clients don't need to see the code they just entered
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
        {
            codePanel.SetActive(true);
            
            // Get the code from the RelayManager
            if (RelayManager.Instance != null)
            {
                codeDisplayField.text = RelayManager.Instance.JoinCode;
            }
        }
        else
        {
            // Hide for Player B
            codePanel.SetActive(false);
        }
    }

    void ClosePanel()
    {
        codePanel.SetActive(false);
    }
}