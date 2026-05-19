# ArcoreGameCore

A reusable Godot 4 + C# game framework for porting and building Arcore games.

## What it is

ArcoreGameCore is a template and plugin library that gives every Arcore game the same backbone:
consistent autoloads, a platform-agnostic adapter layer, reusable visual components, and
data-driven resource definitions. It is designed to make porting Phaser/TypeScript games to
Godot systematic rather than improvisational.

## Project structure

```
ArcoreGameCore/
├── addons/arcore_game_core/   Plugin registration (GDScript @tool)
├── autoload/                  Singleton nodes wired in project.godot
│   ├── platform_bridges/      IPlatformAdapter implementations
│   ├── GameState.cs
│   ├── SaveManager.cs
│   ├── AudioManager.cs
│   ├── InputManager.cs
│   ├── PlatformBridge.cs      Selects adapter at runtime
│   └── SceneRouter.cs
├── scenes/
│   ├── boot/                  Boot scene (preloads, sets up platform)
│   ├── menus/                 MainMenu, PauseMenu, SettingsMenu
│   ├── ui/                    HUD, dialog boxes, score display
│   ├── effects/               DamageNumber, screen flash, transitions
│   └── debug/                 Debug overlay, hitbox visualiser
├── scripts/
│   ├── definitions/           [GlobalClass] Resource definitions
│   └── visual/                Reusable visual components
└── resources/
    ├── game_config/            GameBalanceProfile .tres files
    ├── achievements/           AchievementDefinition .tres files
    ├── input_actions/          InputProfile .tres files
    └── save_schema/            Save data schema definitions
```

## Autoloads

| Singleton | Purpose |
|-----------|---------|
| `GameState` | Score, lives, pause state, chapter tracking |
| `SaveManager` | Slot-based JSON save/load with cloud delegation |
| `AudioManager` | BGM + pooled SFX, per-bus volume control |
| `InputManager` | Named action constants, gamepad detection |
| `PlatformBridge` | Achievement/stat/cloud API, auto-selects adapter |
| `SceneRouter` | Scene changes with transition animations |

## Platform adapters

`PlatformBridge` auto-selects an adapter at boot:

| Adapter | When active |
|---------|-------------|
| `LocalPlatformAdapter` | Desktop without Steam |
| `SteamPlatformAdapter` | When GodotSteam plugin present |
| `WebPlatformAdapter` | When `OS.GetName() == "Web"` |
| `DebugPlatformAdapter` | Debug build with `ARCORE_DEBUG_PLATFORM=1` env var |

## AI-assisted porting system

ArcoreGameCore includes a structured porting pipeline designed for AI-assisted work.

```
porting/
  cli/port-query.js          CLI — tracks porting progress and generates AI briefs
  schema/                    JSON schemas for component and asset manifests
  README.md                  Full CLI usage and AI workflow guide
```

Each game being ported keeps its own manifest alongside its source:
```
<YourGame>/porting/
  port-manifest.json         Every source component: status, deps, Godot target, hours
  asset-manifest.json        Every asset: format, import path, status
  mapping-decisions.md       Log of non-obvious Phaser→Godot decisions
  notes/<component>.md       Detailed AI briefs for complex components
```

**Key command — generate a full AI porting brief for any component:**
```bash
node porting/cli/port-query.js context entity/plumpy
```

This outputs source path, Godot targets, dependency status, assets needed, mapping hints,
and detailed notes — paste it into Claude to start porting with full context loaded.

See [porting/README.md](../porting/README.md) for the complete CLI reference and workflow.

## Getting started

See [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md) for the generic port roadmap.

See [PHASER_TO_GODOT.md](PHASER_TO_GODOT.md) for the Phaser → Godot mapping reference.
