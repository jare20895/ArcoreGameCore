# Export Setup — Windows, Android, Web

## Windows laptop — recommended dev machine

Windows is the lowest-friction environment for Godot C# development and Windows exports:
- No Xvfb / virtual display hacks — Godot runs natively
- Export templates install via Godot's built-in manager (no manual unzip)
- You can run and test Windows builds directly without zip/copy cycles
- No Flatpak sandbox blocking SDK paths

### One-time Windows setup

**Step 1 — Godot 4.4.1 mono**

Download `Godot_v4.4.1-stable_mono_win64.zip` from the GitHub releases page. Extract anywhere (e.g. `C:\Godot\`). No installer needed.

**Step 2 — .NET 8 SDK**

Download the standard installer from microsoft.com/dotnet. Run it. That's it — no PATH tricks needed on Windows.

**Step 3 — Mono export templates**

In Godot: Editor → Manage Export Templates → Download. In the dialog, make sure you select the **mono** template variant (the URL contains `_mono_export_templates.tpz`). The standard templates do NOT include C# support.

Alternatively install manually — download from GitHub releases:
```
Godot_v4.4.1-stable_mono_export_templates.tpz
```
Then in Godot: Editor → Manage Export Templates → Install from file.

**Step 4 — Open the project**

`File → Open Project` → browse to the `godot/` folder → open `project.godot`. Godot will import everything on first open (takes ~30 seconds).

**Step 5 — Test run**

Press F5 or the Play button. The game should boot directly. No export needed for local testing.

**Step 6 — Export**

`Project → Export → Windows Desktop → Export Project` — save to `builds/windows/PlumpyAdventures.exe`. Both the `.exe` and the `data_PlumpyAdventures_windows_x86_64/` folder must stay together.

### Android from Windows

Same setup as Linux — see the Android section below. Android Studio is the easiest way to get the SDK + NDK on Windows.

---

## Prerequisites (Linux / CI / headless)

1. **Godot 4.4.1+ mono binary** — the `mono` / C# build, NOT the standard build.
   - Download: `Godot_v4.4.1-stable_mono_linux_x86_64.zip`
   - The standard non-mono binary cannot run C# exported games.

2. **.NET 8 SDK** (must be findable by Godot — see PATH note below):
   ```bash
   curl -L https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 8.0 --install-dir ~/dotnet
   export PATH="$HOME/dotnet:$PATH"
   ```

3. **Mono export templates** — CRITICAL: there are two `.tpz` files per Godot release:
   - `Godot_v4.4.1-stable_export_templates.tpz` — standard (NO C# support)
   - `Godot_v4.4.1-stable_mono_export_templates.tpz` — **required for C#**

   Download and install the mono one:
   ```bash
   curl -L https://github.com/godotengine/godot/releases/download/4.4.1-stable/Godot_v4.4.1-stable_mono_export_templates.tpz -o templates.tpz
   mkdir -p ~/.local/share/godot/export_templates/4.4.1.stable.mono
   unzip templates.tpz -d /tmp/templates_extract/
   cp /tmp/templates_extract/templates/* ~/.local/share/godot/export_templates/4.4.1.stable.mono/
   ```

   **Verify**: the installed template binary should report `*.mono.*` in `--version`.

---

## Headless export (CI / Linux server)

Use the included script:
```bash
cd <project root>
bash ArcoreGameCore/porting/export-headless.sh [windows|linux|android|all]
```

Set `GODOT_BIN` to your Godot 4.4.1 mono binary path.

### Manual steps

```bash
# 1. Build C# assembly
PATH="$HOME/dotnet:$PATH" dotnet build PlumpyAdventures.csproj -c ExportRelease

# 2. Start virtual display (needed even for headless C# exports)
Xvfb :2 -screen 0 1280x720x24 &

# 3. Export
DISPLAY=:2 PATH="$HOME/dotnet:$PATH" DOTNET_ROOT=$HOME/dotnet \
  godot441_mono --headless --export-release "Windows Desktop" builds/windows/Game.exe
```

### Why DISPLAY is required
The C# bridge (GodotSharp / .NET host) initialises during Godot startup even in headless
mode. Without a real or virtual display Godot skips the mono initialisation step and all
C# autoloads fail silently with "No loader found for resource: res://autoload/Foo.cs".

---

## Flatpak Godot — known limitations

If you installed Godot via Flatpak (`org.godotengine.GodotSharp`):

| Issue | Workaround |
|-------|-----------|
| Export templates are non-mono | Flatpak installs standard templates; copy mono ones manually to `~/.local/share/godot/export_templates/` |
| Java SDK at `/usr/lib/jvm` inaccessible | Use the Flatpak SDK extension path `/usr/lib/sdk/openjdk17/jvm/openjdk-17` in Editor Settings |
| C# Android export blocked with "experimental" error | Use a standalone (non-Flatpak) Godot 4.4.1+ binary |
| `DOTNET_ROOT` not found | Use `JAVA_HOME=/usr/lib/sdk/openjdk17/jvm/openjdk-17` and the flatpak's internal dotnet |

**Recommendation**: Use a standalone Godot 4.4.1 mono binary for headless CI exports. Keep Flatpak for the editor GUI.

---

## Windows

### Setup
No additional tools required beyond Godot + .NET SDK + mono export templates.

The preset in `export_presets.cfg` is pre-configured. Update per game:
```ini
application/company_name="Arcore"
application/product_name="Game Name"
application/copyright="2026 Arcore"
```

### Output structure
```
builds/windows/
  Game.exe                              (94 MB — Godot runtime + embedded PCK)
  data_Game_windows_x86_64/
    Game.dll                            (your C# assembly)
    GodotSharp.dll
    coreclr.dll, hostfxr.dll, ...       (.NET 8 runtime — all bundled)
    System.*.dll                        (~150 .NET BCL assemblies)
```

Both `Game.exe` AND the `data_*` folder must be distributed together. The folder
contains the .NET runtime; without it the .exe crashes immediately after the splash.

### Code signing (optional, removes SmartScreen warning)
Set `codesign/enable=true` and point `codesign/identity` at your `.pfx`. Skip for early builds.

---

## Android

> **Note**: C# Android export is experimental and requires extra steps compared to GDScript games.

### One-time machine setup

**Step 1 — Android SDK + NDK**
```bash
# Download Android command-line tools from developer.android.com/studio
mkdir -p ~/Android/Sdk/cmdline-tools/latest
unzip commandlinetools-linux-*.zip -d ~/Android/Sdk/cmdline-tools/
mv ~/Android/Sdk/cmdline-tools/cmdline-tools/* ~/Android/Sdk/cmdline-tools/latest/

# Install SDK components
JAVA_HOME=/usr/lib/jvm/java-21-openjdk-amd64 \
  ~/Android/Sdk/cmdline-tools/latest/bin/sdkmanager --sdk_root=$HOME/Android/Sdk \
  "platform-tools" "platforms;android-34" "build-tools;34.0.0" "ndk;27.2.12479018"
```

**Step 2 — Java (JDK 17+ — JDK 21 also works)**
```bash
sudo apt install openjdk-21-jdk
```

**Step 3 — .NET Android workload** (required for AOT compilation)
```bash
dotnet workload install android
```

**Step 4 — Debug keystore**
```bash
keytool -genkeypair -v \
  -keystore ~/.android/debug.keystore \
  -alias androiddebugkey \
  -keyalg RSA -keysize 2048 -validity 10000 \
  -storepass android -keypass android \
  -dname "CN=Android Debug,O=Android,C=US"
```

**Step 5 — Godot editor settings** (`~/.local/share/godot/editor_settings-4.3.tres` or equivalent)
```
export/android/java_sdk_path = "/usr/lib/jvm/java-21-openjdk-amd64"
export/android/android_sdk_path = "/home/<user>/Android/Sdk"
export/android/debug_keystore = "/home/<user>/.android/debug.keystore"
export/android/debug_keystore_user = "androiddebugkey"
export/android/debug_keystore_pass = "android"
```

### Android export — headless CI limitation

The Godot CLI `--export-release "Android"` reports:
```
ERROR: Cannot export project with preset "Android" due to configuration errors:
Exporting to Android when using C#/.NET is experimental.
```

This is a hard block in all Godot 4.x headless exports as of 4.4.1. **Workarounds**:
- Export from the Godot editor GUI (works fine — the warning is just shown, not blocking)
- Check Godot 4.5+ releases — this restriction may be lifted

### Per-game setup (export_presets.cfg)
```ini
[preset.N]
name="Android"
platform="Android"
export_path="builds/android/Game.apk"

[preset.N.options]
gradle_build/use_gradle_build=true
gradle_build/min_sdk=24
gradle_build/target_sdk=34
gradle_build/architectures="arm64-v8a"
package/unique_name="com.arcore.gamename"
package/name="Game Name"
keystore/debug="<absolute path to debug.keystore>"
keystore/debug_user="androiddebugkey"
keystore/debug_password="android"
portrait_and_landscape/landscape=true
screen/immersive_mode=true
```

---

## Web

### Setup
No additional tools. Web export templates included with Godot.

### Export steps
```bash
DISPLAY=:2 godot441_mono --headless --export-release "Web" builds/web/index.html
```

### Serving locally (requires HTTPS or localhost for SharedArrayBuffer)
```bash
python3 -m http.server 8080 --directory builds/web
# or
npx serve builds/web
```

### itch.io upload
1. Zip the `builds/web/` folder
2. Upload as HTML game, check "played in browser"
3. Set viewport size to match your window size (e.g. 1280×720)

itch.io automatically sets the required COOP/COEP headers for SharedArrayBuffer/threads.

---

## Common C# export pitfalls

### Assembly name mismatch (game crashes immediately)
`project.godot` `[dotnet] project/assembly_name` MUST exactly match `<AssemblyName>` in `.csproj`.
Space vs no-space causes the PCK to not load C# classes → instant crash.

### Type ambiguity errors
In C# with `ImplicitUsings`, several Godot types conflict with .NET types:
```csharp
// Bad — ambiguous between Godot.Collections.Dictionary and System.Collections.Generic.Dictionary
private Dictionary<string, Array<Dictionary>> _entries;

// Good — fully qualified
private System.Collections.Generic.Dictionary<string, Array<Dictionary>> _entries;

// Same for FileAccess — always use Godot.FileAccess explicitly
var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
```

### Raw data files not auto-included in exports
Files like `.json`, `.csv` that are not referenced from any `.tscn` or `.tres` are NOT
automatically included in the export PCK, even with `include_filter` set.

**Fix options** (pick one):
1. Open the project in the Godot editor at least once — Godot will generate `.import` files
   for recognised resource types (`.json`, etc.) which ARE picked up by the exporter.
2. Use `ResourceLoader.Load<Json>("res://path/to/file.json")` instead of `FileAccess.Open()`
   — the JSON importer marks the file for inclusion.
3. Reference the file from any `.tres` resource that IS included in the export.

### `GD.RandfRange` does not exist in C#
```csharp
// Bad (GDScript name)
GD.RandfRange(0f, 1f)

// Good
(float)GD.RandRange(0.0, 1.0)
```

### SceneTreeTimer is not nullable
```csharp
// Bad — SceneTreeTimer? timer is not nullable in Godot's C# bindings
timer?.SomeMethod()

// Good — use identity check to cancel a pending timer
SceneTreeTimer? _pendingTimer;
var timer = GetTree().CreateTimer(4.0);
_pendingTimer = timer;
timer.Timeout += () => { if (_pendingTimer == timer) DoSomething(); };
```

---

## Build output structure

```
builds/
  windows/
    Game.exe
    data_Game_windows_x86_64/    ← must ship alongside .exe
  linux/
    Game.x86_64
    data_Game_linuxbsd_x86_64/   ← must ship alongside binary
  android/
    Game.apk        (debug / sideload)
    Game.aab        (Play Store release)
  web/
    index.html
    index.js
    index.pck
    index.wasm
```

Add `builds/` to `.gitignore` — build artifacts should not be committed.

---

## Platform support matrix

| Feature | Windows | Linux | Android | Web |
|---------|---------|-------|---------|-----|
| C# / .NET 8 | ✓ JIT | ✓ JIT | ✓ AOT | ✓ AOT |
| SaveManager (file) | ✓ | ✓ | ✓ `user://` | ✓ `user://` (IndexedDB) |
| LocalPlatformAdapter | ✓ | ✓ | ✓ | – |
| SteamPlatformAdapter | ✓ | ✓ | – | – |
| WebPlatformAdapter | – | – | – | ✓ auto-selected |
| GameController input | ✓ | ✓ | ✓ (USB/BT) | ✓ (browser gamepad API) |
| Screen shake | ✓ | ✓ | ✓ | ✓ |
| Audio | ✓ | ✓ | ✓ | ✓ (requires user gesture) |
