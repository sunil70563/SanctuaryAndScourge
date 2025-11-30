
# 🛡️ Sanctuary & Scourge - Asymmetrical Multiplayer Battle

> **Saviors vs. Destroyers: A high-stakes race to save civillians.**

**Sanctuary & Scourge** is a 1v1 asymmetrical multiplayer game built with **Unity** and **Netcode for GameObjects**. One player takes on the role of the **Savior** (Player A), driving an ambulance to rescue civilians, while the other plays as the **Destroyer** (Player B), spreading chaos and infection.

The game is designed to support multiple levels and scenarios. Currently, the first level, **Mushroom Heights**, is fully playable.

![alt text](https://raw.githubusercontent.com/sunil70563/SanctuaryAndScourge/refs/heads/main/Assets/Sprites/banner.png)

## 🗺️ Levels

### Level 1: Mushroom Heights 🍄

Set in a vibrant top-down city, this level introduces the core mechanics of the game.

-   **Setting:** Urban environment with roads, hospitals, and wandering civilians.
    
-   **Challenge:** Navigate tight city streets while avoiding the invisible threat of the infection.
    

_(Future levels will introduce new environments, hazards, and unique power-ups.)_

## 🎮 Gameplay Overview

The match is time-limited (customizable), and victory goes to the player with the highest score when the clock hits zero.

### 🚑 Player A: The Savior (Host)

-   **Objective:** Locate injured civilians, load them into the ambulance, and drive them to the Hospital.
    
-   **Unique Mechanic:** Drives a physical Ambulance vehicle.
    
-   **Scoring:** +1 Point for every civilian successfully delivered.
    
-   **Win Condition:** Rescue more civilians than the Destroyer kills.
    

### 💀 Player B: The Destroyer (Client)

-   **Objective:** Hunt down healthy civilians and attack them.
    
-   **Unique Mechanic:** Invisible "Infection" presence (can use unique movement logic).
    
-   **Scoring:** +1 Point for every civilian that dies after being attacked.
    
-   **Win Condition:** Ensure chaos reigns by letting the infection timer run out on victims.
    

## ⚙️ Core Mechanics

### 1. Civilian Life Cycle

Civilians have dynamic states synced across the network:

-   **Healthy:** Wandering peacefully.
    
-   **Attacked (Yellow):** Hit by Player B. A "Death Timer" starts ticking. They scream for help.
    
-   **In Ambulance:** Timer pauses. Invisible to the world.
    
-   **Saved:** Successfully delivered to the hospital drop zone.
    
-   **Dead (Grey):** Timer ran out before rescue.
    

### 2. Networking Architecture

-   **Unity Relay Service:** Seamless connection without port forwarding.
    
-   **Join Code System:**
    
    -   **Host** creates a match and generates a unique 6-digit code.
        
    -   **Client** enters the code in the Main Menu to join instantly.
        
-   **Scene Management:** Custom `RelayManager` handles data persistence and smooth transitions from Lobby -> Level Select -> Game Scene.
    

### 3. Power-Up System (Inventory & UI)

Both players can collect up to **3 items per type**. These are managed via a dedicated on-screen inventory UI.

**Player A (Saviors)**

**Effect**

⚡ **Speed Booster**

temporarily increases movement speed to outrun the enemy.

🛡️ **Shield**

Grants immunity to Freeze and Shock effects for 10 seconds.

🚁 **Drone Medic**

Instantly teleports the nearest injured civilian into the ambulance.

💊 **Health Kit**

Heals ALL "Attacked" civilians on the map, resetting their death timers.

**Player B (Destroyers)**

**Effect**

❄️ **Freezer**

Freezes Player A in place for 3 seconds (Turn Blue).

⚡ **Shock Drone**

Slows Player A to 50% speed for 5 seconds.

💣 **Bomb**

Instantly kills the nearest "Attacked" civilian.

## 🕹️ Controls & UI

### HUD (Heads-Up Display)

-   **Scoreboard:** Real-time tracking of "Rescued" vs "Infected".
    
-   **Timer:** Match countdown with audio/visual urgency (turns red & ticks) in the last 10 seconds.
    
-   **Zoom Toggle:** Switch between "Follow Cam" and "Tactical Map View".
    

### Interactive Elements

-   **Ambulance Entry/Exit:** Dynamic buttons appear when near the vehicle.
    
-   **Power-Up Panel:** Clickable buttons to activate stored abilities.
    
-   **Options Menu:** Adjust Music/SFX volume (Persistent settings via `GameManager`).
    

## 🛠️ Technical Details

This project demonstrates advanced Unity logic and Networking patterns:

### Key Scripts

-   **`RelayManager.cs`**: The backbone of the connection. Handles Unity Services authentication, allocation, and scene loading.
    
-   **`MatchManager.cs`**: Server-authoritative logic for the game timer, score tracking, and win states.
    
-   **`PlayerMovement.cs`**: Handles inputs, physics, server-side movement validation (RPCs), and power-up execution.
    
-   **`CivilianState.cs`**: complex state machine using `NetworkVariable` to sync health and status to all clients instantly.
    
-   **`PowerUpUIManager.cs`**: Manages the inventory logic, updating UI slots dynamically based on collected items.
    

### Audio System

-   **Spatial Audio:** 3D sound for screams and ambulance sirens.
    
-   **Dynamic Music:** Volume control synced with global settings.
    
-   **Feedback:** Distinct audio cues for picking up items, hitting targets, and low-time warnings.
    

## 🚀 How to Play

1.  **Main Menu:**
    
    -   Player A (Host) clicks **"Create Match"**.
        
    -   Player A sets the **Match Duration** (e.g., 3 Minutes).
        
2.  **Lobby:**
    
    -   The game generates a **Join Code** (displayed in a popup).
        
    -   Player A shares this code with Player B.
        
3.  **Joining:**
    
    -   Player B enters the code in the **"Join Game"** field and connects.
        
4.  **The Match:**
    
    -   **A:** Drive fast! Use the map to find yellow (injured) civilians.
        
    -   **B:** Attack! Use the Bomb power-up to secure kills.
        
5.  **Game Over:**
    
    -   When the timer hits 00:00, the winner is declared based on the final score.
        

**Current Status:**

-   **Project:** Sanctuary & Scourge
    
-   **Active Level:** Level 1 - Mushroom Heights (Complete)
