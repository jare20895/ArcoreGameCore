# Backlog

Codebase review date: 2026-05-28

## Stabilize Template Runtime

- Add a minimal runnable boot flow: `Boot.tscn` loads core resources, initializes platform state, and routes to `MainMenu.tscn`.
- Add placeholder menu and UI scenes for every path hard-coded in `SceneRouter`.
- Add a smoke-test checklist for opening the project in Godot, running the boot scene, changing scenes, saving/loading, and playing one SFX/BGM.
- Add a small sample gameplay scene that demonstrates `GameState`, `InputManager`, `AudioManager`, `SaveManager`, and `SceneRouter` together.

## Save, Settings, and Persistence

- Refactor `SaveManager` around a typed save envelope with `version`, `timestamp`, `slot`, and `payload`.
- Add save migration hooks keyed by save version.
- Add slot metadata listing so menus can show empty/occupied slots without loading every full save.
- Add settings persistence for audio volume, mute state, display options, and input bindings.
- Add cloud save read support to `IPlatformAdapter`; current API only writes cloud files.
- Add `SaveFailed`, `LoadFailed`, and `SlotDeleted` signals with error details.

## Platform Integrations

- Implement a real GodotSteam adapter or make Steam selection opt-in until it is complete.
- Harden `WebPlatformAdapter` by escaping all JS arguments and persisting web achievement unlock state.
- Add platform capability flags such as `SupportsAchievements`, `SupportsCloudSave`, `SupportsLeaderboard`, and `SupportsRichPresence`.
- Add a platform adapter test harness that can exercise all adapter methods in debug builds.
- Add local leaderboard/stat persistence for desktop fallback mode.

## Input and Accessibility

- Add input rebinding APIs and a resource-backed input profile.
- Register default gamepad bindings alongside keyboard bindings.
- Add controller glyph lookup for common Xbox, PlayStation, and keyboard prompts.
- Add accessibility toggles for screen shake, flash intensity, text speed, and hold/toggle controls.
- Add deadzone and vibration settings for gamepad users.

## Audio

- Implement BGM fade-in, fade-out, and crossfade support.
- Add SFX volume and pitch controls per call, not just via global bus state.
- Add bus creation/validation guidance for projects that do not already have `BGM` and `SFX` buses.
- Add music state helpers for pause/resume and scene transitions.

## Visual Utilities

- Add the missing `DamageNumber.tscn` scene and make `DamageNumber.Spawn` initialization safe.
- Expand `ScreenTransition` to support fade, wipe, and instant modes.
- Add screen flash and hit-pause utilities for common action-game feedback.
- Fix `CameraShake` offset reset and add amplitude/frequency exports.
- Complete `ParallaxBackground` with vertical tiling and camera-follow influence.
- Add particle preset scenes for dust, sparks, splash, and pickup effects.

## Data-Driven Gameplay

- Add validation helpers for `CharacterDefinition`, `EnemyDefinition`, `LevelDefinition`, `AbilityDefinition`, and `AchievementDefinition`.
- Add sample `.tres` resources under `resources/` so users can inspect expected data shapes in the editor.
- Add a registry/loader for definitions by ID.
- Add achievement progress tracking based on `AchievementDefinition.StatTracked` and `StatThreshold`.
- Add level progression helpers using `LevelDefinition.NextLevel` and `UnlockRequirement`.

## Porting Tooling

- Add a sample `port-manifest.json` and `asset-manifest.json` for this repo so the CLI works without a sibling project.
- Allow `port-query.js` defaults to come from an env var or config file.
- Add JSON schema validation to `port-query.js` before command execution.
- Add a `doctor` command that checks manifest paths, missing notes files, missing Godot targets, and unresolved dependencies.
- Add a `scaffold` command that creates target scene/script folders from manifest entries.
- Add CI tests for CLI commands using a small fixture manifest.

## Testing and CI

- Add Godot C# unit tests for `GameState`, `SaveManager`, `SceneRouter`, and platform adapter selection.
- Add Node.js tests for `port-query.js`.
- Add a CI workflow that runs Node CLI tests and .NET/Godot build checks where the Godot mono toolchain is available.
- Add lint/format guidance for C# and GDScript files.
- Add export smoke checks for desktop and web presets once the boot scene exists.

## Documentation

- Add a "known limitations" section to the root README.
- Document exact Godot version alignment; project uses Godot 4.3 SDK while export docs reference 4.4.1.
- Document required audio buses and autoload behavior.
- Add per-system usage examples for saving, scene routing, achievements, and audio.
- Add a starter-port checklist that distinguishes framework setup from per-game implementation.
