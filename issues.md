# Issues

Codebase review date: 2026-05-28

## High Priority

### Missing main scene prevents the template from running

- Evidence: `project.godot` sets `run/main_scene="res://scenes/boot/Boot.tscn"`, and `SceneRouter.Scenes.Boot` points to the same path, but `scenes/` contains no files.
- Impact: a fresh clone cannot be launched as a usable Godot project. Any port starting from this template must create the boot scene before basic smoke testing is possible.
- Suggested fix: add minimal `Boot.tscn`, `MainMenu.tscn`, `PauseMenu.tscn`, `HUD.tscn`, and `DamageNumber.tscn` scenes that match the paths in `SceneRouter`.

### `SaveManager.Save` can upload empty cloud data

- Evidence: `SaveManager.Save` writes JSON, then immediately calls `file.GetBuffer((long)file.GetLength())` on the same write-mode file handle.
- Impact: depending on Godot file cursor semantics, the cloud adapter can receive no bytes or partial bytes after local save succeeds. Local save and cloud save can silently diverge.
- Suggested fix: serialize once to a string, write that string locally, then pass `Encoding.UTF8.GetBytes(json)` to `PlatformBridge.SaveCloudFile`.

### Web adapter injects unsanitized values into JavaScript strings

- Evidence: `WebPlatformAdapter` interpolates `id`, `path`, `boardId`, and base64 data directly into `JavaScriptBridge.Eval(...)`.
- Impact: achievement IDs, save paths, or leaderboard IDs containing quotes/backslashes can break the script; user-controlled values could become script injection.
- Suggested fix: add a JS string escaping helper or use JSON serialization for arguments before interpolation.

### Web achievements are never persisted on unlock

- Evidence: `WebPlatformAdapter.UnlockAchievement` only logs; `IsAchievementUnlocked` checks `localStorage.getItem('ach_{id}')`.
- Impact: unlocking an achievement on web will still report false later unless another external script writes the storage key.
- Suggested fix: write `localStorage.setItem('ach_<id>', '1')` in `UnlockAchievement`, or delegate both unlock/query to a real platform API with a local fallback.

### Steam adapter is auto-selected but only a stub

- Evidence: `PlatformBridge.ResolveAdapter` selects `SteamPlatformAdapter` when `ClassDB.ClassExists("Steam")`, while `SteamPlatformAdapter` mostly logs/no-ops and `IsAchievementUnlocked` always returns false.
- Impact: installing GodotSteam changes runtime behavior from working local persistence to non-persistent no-op platform calls.
- Suggested fix: either implement the GodotSteam calls before auto-selecting it, or gate Steam selection behind explicit project setting/env opt-in until complete.

## Medium Priority

### Editor plugin can remove user-managed autoloads with matching names

- Evidence: `addons/arcore_game_core/plugin.gd` always calls `remove_autoload_singleton("GameState")` and the other singleton names on plugin exit.
- Impact: if a game customizes one of these autoloads or already had a singleton with the same name, disabling the plugin can remove the user's project setting.
- Suggested fix: on `_enter_tree`, record whether each autoload was added by the plugin and only remove matching paths on `_exit_tree`.

### `SceneRouter` emits completion without checking scene-change failure

- Evidence: `SceneRouter.DoSceneChange` ignores the return value from `GetTree().ChangeSceneToFile(path)` and always emits `SceneChangeCompleted`.
- Impact: callers can believe navigation succeeded even when the target path is missing or invalid.
- Suggested fix: inspect the returned `Error`, log failures, keep or expose failure state, and only emit completion on `Error.Ok`.

### `DamageNumber.Spawn` dereferences `_label` before `_Ready`

- Evidence: `DamageNumber.Spawn` instantiates the scene, adds it to the parent, then immediately accesses `instance._label.Text`.
- Impact: if `_Ready` has not initialized `_label` yet, spawning a damage number can throw a null reference.
- Suggested fix: add an `Initialize(text, color)` method that stores pending values before `_Ready`, or set exported properties and apply them in `_Ready`.

### `AudioManager.PlayBgm` accepts loop/fade parameters but ignores them

- Evidence: `PlayBgm(AudioStream stream, bool loop = true, float fadeIn = 0f)` does not use either `loop` or `fadeIn`.
- Impact: callers will assume fade and loop behavior exists, but music changes abruptly and looping depends entirely on stream import settings.
- Suggested fix: either implement fade-in/out and document import-time loop handling, or remove the unused parameters until supported.

### `ParallaxBackground` exposes unused camera influence and has vertical-scroll gaps

- Evidence: `CameraInfluence` is exported but unused; `_Process` applies Y offset without creating vertical tile coverage.
- Impact: designers can tune a setting that does nothing, and vertical scrolling backgrounds can reveal empty space.
- Suggested fix: either implement camera-relative offset and Y tiling, or remove/rename unsupported exports.

### `CameraShake` leaves the camera offset at the last random value

- Evidence: `_Process` returns immediately when `_trauma <= 0f` and never restores `_camera.Offset` to `Vector2.Zero`.
- Impact: after a shake decays, the camera can remain subtly displaced.
- Suggested fix: when trauma reaches zero, reset offset to zero; also validate that the parent is actually a `Camera2D`.

## Low Priority

### `LocalPlatformAdapter.Persist` does not handle file-open failure

- Evidence: `Persist` calls `Godot.FileAccess.Open(...).StoreString(...)` without checking for null.
- Impact: a failed write becomes a null reference instead of a clear platform persistence error.
- Suggested fix: mirror the null check used by `SaveManager.Save`.

### CLI defaults are hard-coded to `PlumpyAdventures`

- Evidence: `porting/cli/port-query.js` defaults to `../../../PlumpyAdventures/porting/port-manifest.json`.
- Impact: running documented commands from this framework repo fails unless the sibling project exists.
- Suggested fix: support a local `.arcore-port.json`, an `ARCORE_PORT_MANIFEST` env var, or scaffold an example manifest in this repo.

### Docs reference completed foundation pieces that are not present

- Evidence: `docs/IMPLEMENTATION_PLAN.md` checks off core autoloads/utilities, but also lists required scenes that are still absent.
- Impact: readers may overestimate template readiness and discover missing pieces only at runtime.
- Suggested fix: add a short "current template limitations" section or keep checklist status synchronized with committed files.

## Verification Notes

- `node porting/cli/port-query.js --help` runs successfully.
- `dotnet build ArcoreGameCore.csproj` could not be run in this environment because `dotnet` is not installed.
