using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PowerUpUIManager : MonoBehaviour
{
    public static PowerUpUIManager Instance { get; private set; }

    [Header("Player A Panel (Saviors)")]
    public GameObject panelPlayerA;
    // 1. Speed
    public Button btnSpeed;
    public TMP_Text txtSpeed;
    // 2. Shield
    public Button btnShield;
    public TMP_Text txtShield;
    // 3. Medic
    public Button btnMedic;
    public TMP_Text txtMedic;
    // 4. Health
    public Button btnHealth;
    public TMP_Text txtHealth;

    [Header("Player B Panel (Destroyers)")]
    public GameObject panelPlayerB;
    // 1. Freezer
    public Button btnFreezer;
    public TMP_Text txtFreezer;
    // 2. Shock
    public Button btnShock;
    public TMP_Text txtShock;
    // 3. Bomb
    public Button btnBomb;
    public TMP_Text txtBomb;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if(panelPlayerA) panelPlayerA.SetActive(false);
        if(panelPlayerB) panelPlayerB.SetActive(false);

        // --- Add Listeners for Player A ---
        if(btnSpeed) btnSpeed.onClick.AddListener(() => OnPowerUpClicked(PowerUpType.SpeedBooster));
        if(btnShield) btnShield.onClick.AddListener(() => OnPowerUpClicked(PowerUpType.Shield));
        if(btnMedic) btnMedic.onClick.AddListener(() => OnPowerUpClicked(PowerUpType.DroneMedic));
        if(btnHealth) btnHealth.onClick.AddListener(() => OnPowerUpClicked(PowerUpType.HealthKit));

        // --- Add Listeners for Player B ---
        if(btnFreezer) btnFreezer.onClick.AddListener(() => OnPowerUpClicked(PowerUpType.Freezer));
        if(btnShock) btnShock.onClick.AddListener(() => OnPowerUpClicked(PowerUpType.ShockDrone));
        if(btnBomb) btnBomb.onClick.AddListener(() => OnPowerUpClicked(PowerUpType.Bomb));
    }

    private void OnPowerUpClicked(PowerUpType type)
    {
        var players = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        foreach(var p in players)
        {
            if (p.IsOwner)
            {
                p.AttemptToActivatePowerUp(type);
                break;
            }
        }
    }

    public void UpdateCount(PowerUpType type, int currentCount, int maxCount)
    {
        string countText = $"{currentCount}/{maxCount}";
        bool isUsable = currentCount > 0;

        switch (type)
        {
            // Player A
            case PowerUpType.SpeedBooster:
                if(txtSpeed) txtSpeed.text = countText;
                if(btnSpeed) btnSpeed.interactable = isUsable;
                break;
            case PowerUpType.Shield:
                if(txtShield) txtShield.text = countText;
                if(btnShield) btnShield.interactable = isUsable;
                break;
            case PowerUpType.DroneMedic:
                if(txtMedic) txtMedic.text = countText;
                if(btnMedic) btnMedic.interactable = isUsable;
                break;
            case PowerUpType.HealthKit:
                if(txtHealth) txtHealth.text = countText;
                if(btnHealth) btnHealth.interactable = isUsable;
                break;

            // Player B
            case PowerUpType.Freezer:
                if(txtFreezer) txtFreezer.text = countText;
                if(btnFreezer) btnFreezer.interactable = isUsable;
                break;
            case PowerUpType.ShockDrone:
                if(txtShock) txtShock.text = countText;
                if(btnShock) btnShock.interactable = isUsable;
                break;
            case PowerUpType.Bomb:
                if(txtBomb) txtBomb.text = countText;
                if(btnBomb) btnBomb.interactable = isUsable;
                break;
        }
    }

    public void ShowPanelForPlayer(bool isPlayerA)
    {
        if(panelPlayerA) panelPlayerA.SetActive(isPlayerA);
        if(panelPlayerB) panelPlayerB.SetActive(!isPlayerA);
    }
}