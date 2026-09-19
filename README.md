# 🔮 Magic Arena

> **Wave-based VR combat with puzzle-driven progression and responsive spellcrafting tuned for immersion.**

![Unity](https://img.shields.io/badge/Unity-6.x-black?logo=unity) ![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white) ![VR](https://img.shields.io/badge/Platform-Meta%20Quest-blue?logo=meta) ![Status](https://img.shields.io/badge/Status-Complete-brightgreen)

---

## 🎮 Overview

Magic Arena is a VR combat game built for Meta Quest, combining wave-based enemy encounters with voice-activated spellcasting and puzzle-gated progression. Players fight through 3 levels — each containing 3 waves of enemies — then solve an environmental puzzle to unlock the next level.

The core design goal was full immersion: spells are cast with your voice, enemies adapt to your pacing, and the puzzle and combat systems are tightly interwoven into a single cohesive loop.

---

## 🏗️ Structure

```
3 Levels
└── Level 1
│   ├── Wave 1 — Basic enemies
│   ├── Wave 2 — Mixed enemy types
│   ├── Wave 3 — Elite enemies
│   └── 🧩 Puzzle Gate → unlocks Level 2
└── Level 2
│   ├── Wave 1–3 (increased difficulty)
│   └── 🧩 Puzzle Gate → unlocks Level 3
└── Level 3
    ├── Wave 1–3 (final challenge)
    └── 🧩 Final Puzzle Gate → Victory
```

---

## ✨ Core Features

### 🔥 Elemental Spellcasting
- Multiple elemental abilities — fire, ice, lightning and more
- Unique combo system — combining elements triggers enhanced effects
- Cooldown management adds strategic depth to combat

### 🎙️ Voice-Activated Casting
- Hands-free spell activation via voice commands
- Voice command flow designed for fast, intense combat sessions
- Cooldown feedback keeps the loop responsive and fair

### 🧩 Puzzle-Driven Progression
- Each level is locked behind a puzzle gate
- Puzzles are tied to combat mechanics — solutions emerge from gameplay
- Prevents the combat and exploration loops from feeling disconnected

### 🤖 Adaptive Enemy AI
- Enemies adjust behavior and pacing based on player performance
- AI tuned for short, intense sessions — no filler, no downtime
- Different mob types per wave keep each encounter fresh

---

## 🎯 Design Objectives

| ID | Objective | Status |
|---|---|---|
| OBJ.01 | Elemental abilities with unique combos | ✅ Complete |
| OBJ.02 | Voice-assisted spell activation and cooldowns | ✅ Complete |
| OBJ.03 | Puzzle gates tied to combat progression | ✅ Complete |
| OBJ.04 | Adaptive enemy AI behaviors | ✅ Complete |

---

## 🛠️ Tech Stack

| Technology | Usage |
|---|---|
| Unity (C#) | Core engine and gameplay systems |
| Meta Quest SDK | VR input, hand tracking, headset integration |
| Unity XR Toolkit | XR interaction framework |
| Unity AI / NavMesh | Enemy pathfinding and behavior |
| Voice Recognition API | Hands-free spell activation |

---

## 🚀 Getting Started

### Prerequisites
- Unity 6.x
- Meta Quest Developer Hub
- Meta XR SDK installed in Unity

### Run on Device
```bash
# Clone the repository
git clone https://github.com/Salmiselim/magic-arena.git

# Open in Unity 6.x
# Build target: Android (Meta Quest)
# Enable XR Plugin: Oculus

# Deploy via Meta Quest Developer Hub
# or: File → Build & Run
```

---

## 📁 Project Structure

```
Assets/
├── Scripts/
│   ├── Combat/          # Spell system, combos, cooldowns
│   ├── AI/              # Enemy behaviors, wave manager
│   ├── Puzzles/         # Puzzle gate logic per level
│   ├── Voice/           # Voice command recognition
│   └── Core/            # Game manager, level flow
├── Prefabs/
│   ├── Enemies/         # Mob types per wave
│   ├── Spells/          # Elemental VFX + logic
│   └── Puzzles/         # Gate prefabs per level
├── Scenes/
│   ├── Level_01
│   ├── Level_02
│   └── Level_03
└── XR/                  # Meta Quest XR configuration
```

---

## 👨‍💻 Author

**Selim Salmi**
- 🌐 [selim-salmi.tn](https://selim-salmi.tn)
- 💼 [linkedin.com/in/selim-salmi](https://linkedin.com/in/selim-salmi)
- 🐱 [github.com/Salmiselim](https://github.com/Salmiselim)

---

*Built as part of a Master's-level software engineering program at ESPRIT Tunisia.*
