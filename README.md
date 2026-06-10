# Project-Legend-Zunni-Core-Systems

C# source code from *Project Legend: Zunni* — a 2D incremental action game built in Unity.
This repo showcases individual systems pulled from the live project, each refactored for clarity, DRY architecture, and scalability.

---

## Systems Checklist

### ✅ Done & Uploaded
- [X] `ShopManager.cs` — Infinite-scaling shop with x1/x10/x100/Max multipliers (BreakInfinity)
- [X] `PlayerStats.cs` — Movement, combat, tiered knockback, and hit feedback
- [X] `PlayerSavedData.cs` — cross-session save container for shop prices and player state
- [X] `PlayerController.cs` — input, movement, animation, and knockback with DRY architecture
- [X] `AudioPlayer.cs` — SFX playback, music ducking, hurt scaling, and boss music transitions
- [X] `StageData` — ScriptableObject defining stage identity, scene objects, and win condition
- [X] `CurrencyPickUp` — magnetic homing pickup with loot pop, sparkle particles, and animation override


---

### 🔊 Audio
- [X] `AudioPlayer.cs`

### ⚙️ Core
- [ ] `BigDouble.cs`
- [ ] `GameManager.cs`
- [X] `StageData.cs`

### 👾 Enemies
- [ ] `BossController.cs`
- [ ] `BossSetActiveTrigger.cs`
- [ ] `Enemy.cs`
- [ ] `EnemyData.cs`
- [ ] `EnemyTriggerThatSpawnsEnemies.cs`
- [ ] `NormalEnemiesSpawner.cs`

### 💎 Entities & Pickups
- [ ] `CurrencyData.cs`
- [X] `CurrencyPickUp.cs`
- [ ] `XPOrb.cs`

### 🌍 Environment & Camera
- [ ] `Background.cs`
- [ ] `CameraShake.cs`
- [ ] `FollowPlayer.cs`
- [ ] `AttachToPlayersCamera.cs`

### 🗂️ Managers
- [ ] `CurrencyManager.cs`
- [ ] `SpeedManager.cs`
- [X] `ShopManager.cs`

### 🧍 Player
- [X] `PlayerStats.cs`
- [X] `PlayerController.cs`
- [X] `PlayerSavedData.cs`

### 🖥️ UI
- [ ] `DamageText.cs`
- [ ] `EnemyHealthBar.cs`
- [ ] `HealthBar.cs`
- [ ] `SpawningUI.cs`
- [ ] `XPBarUI.cs`

---

## Dependencies
- [BreakInfinity.cs](https://github.com/Razenpok/BreakInfinity.cs)
- [Feel / MoreMountains Feedbacks](https://feel.moremountains.com/)
- TextMeshPro *(included with Unity)*

## Engine
Unity 2022.x — 2D project

---

## Why This Repo Exists
Each script here represents a real system from a shipped game build.
The goal is clean, readable, production-quality C# — not tutorial code.
