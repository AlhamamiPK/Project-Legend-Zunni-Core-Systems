# Project-Legend-Zunni-Core-Systems

C# source code from *Project Legend: Zunni* — a 2D incremental action game built in Unity.
This repo showcases individual systems pulled from the live project, each refactored for clarity, DRY architecture, and scalability.

---

## Systems Checklist

### ✅ Done & Uploaded
- [x] `ShopManager.cs` — Infinite-scaling shop with x1/x10/x100/Max multipliers (BreakInfinity)
- [x] `PlayerStats.cs` — Movement, combat, tiered knockback, and hit feedback

---

### 🔊 Audio
- [ ] `AudioPlayer.cs`

### ⚙️ Core
- [ ] `BigDouble.cs`
- [ ] `GameManager.cs`
- [ ] `StageData.cs`

### 👾 Enemies
- [ ] `BossController.cs`
- [ ] `BossSetActiveTrigger.cs`
- [ ] `Enemy.cs`
- [ ] `EnemyData.cs`
- [ ] `EnemyTriggerThatSpawnsEnemies.cs`
- [ ] `NormalEnemiesSpawner.cs`

### 💎 Entities & Pickups
- [ ] `CurrencyData.cs`
- [ ] `CurrencyPickUp.cs`
- [ ] `XPOrb.cs`

### 🌍 Environment & Camera
- [ ] `Background.cs`
- [ ] `CameraShake.cs`
- [ ] `FollowPlayer.cs`
- [ ] `AttachToPlayersCamera.cs`

### 🗂️ Managers
- [ ] `CurrencyManager.cs`
- [ ] `SpeedManager.cs`

### 🧍 Player
- [ ] `CustomMouse.cs`
- [ ] `PlayerController.cs`
- [ ] `PlayerSavedData.cs`

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
