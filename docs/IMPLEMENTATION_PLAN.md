# ArcoreGameCore — Implementation Plan

## Goal

Build and maintain a reusable Godot 4 / C# framework that any Phaser 3 / TypeScript web game
can be ported into. Each new port starts from the same backbone rather than from scratch.

---

## Phase 1 — Framework foundation

**Goal:** Stable, tested core that every game can drop into.

### Deliverables

- [x] Project scaffold (`project.godot`, `ArcoreGameCore.csproj`)
- [x] Autoloads: GameState, SaveManager, AudioManager, InputManager, PlatformBridge, SceneRouter
- [x] Platform adapters: Local, Steam (stub), Web, Debug
- [x] Visual utilities: ScreenTransition, CameraShake, DamageNumber, ParallaxBackground
- [x] Resource definitions: Character, Enemy, Level, Ability, Achievement, GameBalanceProfile
- [x] Editor plugin (`addons/arcore_game_core`)
- [ ] Boot scene (`scenes/boot/Boot.tscn`) — preloads assets, selects platform adapter
- [ ] Main menu scene (`scenes/menus/MainMenu.tscn`) — start, continue, settings
- [ ] Pause menu scene (`scenes/menus/PauseMenu.tscn`) — resume, restart, quit
- [ ] HUD scene (`scenes/ui/HUD.tscn`) — score, lives, chapter indicator
- [ ] Settings menu — audio sliders, input rebinding, display options
- [ ] Unit tests for SaveManager and GameState (GUT framework)

### Exit criteria
All autoloads initialise without errors on both desktop and web export targets.
SaveManager round-trips data correctly. PlatformBridge selects the right adapter automatically.

---

## Phase 2 — Per-game port scaffold

When starting a new game port, fork or copy this project and complete:

### Checklist for each new port

**Player**
- [ ] Player controller (`CharacterBody2D`) matching source game movement feel
- [ ] Input actions mapped to game verbs (jump, attack, interact, etc.)
- [ ] `CharacterDefinition` resource for each playable character

**Enemies**
- [ ] Enemy base class (`CharacterBody2D` or `Area2D`) with `EnemyDefinition` resource
- [ ] Per-enemy behaviour scripts (patrol, chase, projectile, etc.)
- [ ] Death effects wired to `EnemyDefinition.DeathEffect`

**Levels**
- [ ] Tilemap(s) imported from source art (`TileMapLayer` per layer)
- [ ] `LevelDefinition` resources with chaining (`NextLevel`)
- [ ] Background layers (`ParallaxBackground`) matching source parallax

**Systems**
- [ ] Dialogue / cutscene system (if source game uses dialogue)
- [ ] Collectibles / pickups
- [ ] Checkpoint / respawn logic
- [ ] Chapter-complete / game-over screens

**Audio**
- [ ] BGM stream assets imported and wired to `LevelDefinition.Bgm`
- [ ] SFX assets imported and referenced from entity/effect scripts

**Polish**
- [ ] Screen shake triggered on impacts
- [ ] Damage numbers on hit
- [ ] Transition animations between levels

### Exit criteria
First level playable start to finish. SaveManager persists progress. Audio plays correctly.

---

## Phase 3 — Full content port

Port all remaining levels/chapters from the source game using the pattern from Phase 2.

- One `LevelDefinition` resource per level
- Shared entity scenes reused across levels (no duplication)
- Boss encounters as self-contained scenes referenced from `LevelDefinition`
- All dialogue/cutscene scripts ported

### Exit criteria
Game completable from start to credits. Feature-parity with Phaser version.

---

## Phase 4 — Visual upgrade pass

Apply shared visual polish from the framework:

- [ ] Shader materials for environmental effects (water, fire, glass, etc.)
- [ ] 2D point lights for torches, glowing enemies, pickups
- [ ] Hit-pause (1–3 frame freeze on impact via `Engine.TimeScale`)
- [ ] Screen flash on damage received
- [ ] Controller glyph prompts (swap icons based on `InputManager.IsGamepad`)
- [ ] Animated scene transitions (wipe, circle-reveal, fade)
- [ ] Particle burst presets (dust, sparks, splash) via `GPUParticles2D`

---

## Phase 5 — Platform & release targets

- [ ] Web export (itch.io) — verify `WebPlatformAdapter` (localStorage, JS bridge)
- [ ] Desktop export (Windows / Linux / Mac)
- [ ] Steam integration — wire `SteamPlatformAdapter` with GodotSteam plugin
- [ ] Achievements wired to gameplay events via `PlatformBridge.UnlockAchievement()`
- [ ] Cloud save via `PlatformBridge.SaveCloudFile()`
- [ ] Leaderboard (optional) via `PlatformBridge.SubmitScore()`

---

## Phase 6 — Framework refinement

After each completed port, feed improvements back into ArcoreGameCore:

- [ ] Extract any game-specific patterns that proved reusable
- [ ] Add new visual utilities or resource definitions as needed
- [ ] Update `PHASER_TO_GODOT.md` with any new mapping discoveries
- [ ] Tag a release version of the framework

---

## Technical decisions

### Why C# over GDScript
- Matches existing Arcore tooling (C# across ArcoreAgent, ArcoreFactory, etc.)
- Better IDE support (Rider / VS Code with OmniSharp)
- Stronger typing reduces porting errors when translating from TypeScript

### Why Godot 4.3+
- `TileMapLayer` replaces deprecated `TileMap` — cleaner per-layer API
- Improved C# / .NET 8 support
- Better GPU particles and shader API for visual upgrade work

### Save format
JSON via Godot's built-in `Json` class. One file per slot (`user://saves/slot_N.json`).
Schema version field in every save enables forward-compatible migrations.

### Scene organisation convention
```
scenes/
  boot/          — Boot.tscn (always first scene)
  menus/         — MainMenu, PauseMenu, GameOver, Settings
  ui/            — HUD, DialogueBox, AchievementToast
  effects/       — DamageNumber, ScreenFlash, ParticleBursts
  entities/      — Player, Enemy base scenes (shared across levels)
  levels/        — One subfolder per level/chapter
  debug/         — DebugOverlay, HitboxVisualiser
```
Shared entities live in `scenes/entities/` and are instantiated by level scenes.
Level-specific content lives in `scenes/levels/<id>/`.
