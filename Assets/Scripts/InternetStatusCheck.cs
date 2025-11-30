using UnityEngine;
using TMPro; // Required for TextMeshPro
using System.Collections; // Required for Coroutines

[RequireComponent(typeof(TextMeshProUGUI))]
public class InternetStatusDisplay : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("How often (in seconds) to check the connection.")]
    [SerializeField] private float checkInterval = 5.0f;

    // --- NEW! Public color fields ---
    [Header("Display Colors")]
    [Tooltip("The color to display when internet is connected.")]
    public Color connectedColor = Color.white;

    [Tooltip("The color to display when internet is NOT connected.")]
    public Color notConnectedColor = Color.red;
    // --- END NEW ---

    // --- Private Variables ---
    private TextMeshProUGUI statusText;
    private Coroutine checkCoroutine;

    // Your custom colors
    // (These are now public variables above)

    private void Start()
    {
        // Automatically get the component on this GameObject
        statusText = GetComponent<TextMeshProUGUI>();

        // Start the repeating check
        checkCoroutine = StartCoroutine(CheckInternetStatusRoutine());
    }

    private void OnDisable()
    {
        // Stop the coroutine if the object is disabled or destroyed
        if (checkCoroutine != null)
        {
            StopCoroutine(checkCoroutine);
        }
    }

    /// <summary>
    /// Coroutine that checks the internet status every 'checkInterval' seconds.
    /// </summary>
    private IEnumerator CheckInternetStatusRoutine()
    {
        // Loop indefinitely while the script is active
        while (true)
        {
            UpdateStatusText();
            
            // Wait for the specified interval before checking again
            yield return new WaitForSeconds(checkInterval);
        }
    }

    /// <summary>
    /// Checks Application.internetReachability and updates the text.
    /// </summary>
    private void UpdateStatusText()
    {
        string message = "";
        Color statusColor = Color.white; // Default, will be overwritten

        // Check the current network reachability status
        switch (Application.internetReachability)
        {
            case NetworkReachability.NotReachable:
                message = ">> NO INTERNET <<     .";
                statusColor = notConnectedColor; // Use public variable
                break;

            case NetworkReachability.ReachableViaCarrierDataNetwork:
                message = "<< Connected (Mobile Data) >>   .";
                statusColor = connectedColor; // Use public variable
                break;

            case NetworkReachability.ReachableViaLocalAreaNetwork:
                message = "<< Connected (WiFi/Ethernet) >>   .";
                statusColor = connectedColor; // Use public variable
                break;
        }

        // Update the text and color
        statusText.text = message;
        statusText.color = statusColor;
    }
}