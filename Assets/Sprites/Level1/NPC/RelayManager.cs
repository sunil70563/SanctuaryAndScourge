using UnityEngine;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }

    public string JoinCode { get; private set; }

    // Internal data to hold connection info until the scene loads
    private bool isHost = false;
    private Allocation hostAllocation;
    private JoinAllocation clientAllocation;
    private bool needsToStartNetwork = false;

    [Header("Settings")]
    public string gameSceneName = "MushromHeights";

    private void Awake()
    {
        // Singleton that survives scene loads
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        try 
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Unity Services Init Error: " + e.Message);
        }
        
        // Listen for scene changes so we know when the Game Scene finishes loading
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // --- STEP 1: PREPARE DATA (Called from Menu) ---

    // Host Flow
    public async Task<string> CreateRelay()
    {
        try
        {
            // Request allocation for 2 players
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
            // Save data locally
            this.JoinCode = joinCode;
            this.hostAllocation = allocation;
            this.isHost = true;
            this.needsToStartNetwork = true;

            // Load the Game Scene manually
            SceneManager.LoadScene(gameSceneName);

            return joinCode;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Relay Create Error: " + e.Message);
            return null;
        }
    }

    // Client Flow - UPDATED to return Task<bool>
    public async Task<bool> JoinRelay(string joinCode)
    {
        try
        {
            // Join allocation
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            
            // Save data locally
            this.clientAllocation = allocation;
            this.isHost = false;
            this.needsToStartNetwork = true;

            // Load the Game Scene manually
            SceneManager.LoadScene(gameSceneName);
            
            return true; // Success!
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Relay Join Error: " + e.Message);
            return false; // Failed (Wrong code or internet issue)
        }
    }

    // --- STEP 2: CONFIGURE NETWORK MANAGER (Called when Scene Loads) ---

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Only run if we actually have a pending connection request
        if (!needsToStartNetwork) return;

        // Check if we are in the correct scene
        if (scene.name == gameSceneName)
        {
            StartGameConnection();
        }
    }

    private void StartGameConnection()
    {
        // Find the newly spawned NetworkManager in the Game Scene
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError($"NetworkManager missing in {gameSceneName}!");
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        if (isHost)
        {
            transport.SetHostRelayData(
                hostAllocation.RelayServer.IpV4,
                (ushort)hostAllocation.RelayServer.Port,
                hostAllocation.AllocationIdBytes,
                hostAllocation.Key,
                hostAllocation.ConnectionData
            );
            NetworkManager.Singleton.StartHost();
            Debug.Log("Started Host via Relay.");
        }
        else
        {
            transport.SetClientRelayData(
                clientAllocation.RelayServer.IpV4,
                (ushort)clientAllocation.RelayServer.Port,
                clientAllocation.AllocationIdBytes,
                clientAllocation.Key,
                clientAllocation.ConnectionData,
                clientAllocation.HostConnectionData
            );
            NetworkManager.Singleton.StartClient();
            Debug.Log("Started Client via Relay.");
        }

        // Reset flag so we don't try to start again
        needsToStartNetwork = false;
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}