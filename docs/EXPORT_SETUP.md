# Export Setup — Windows, Android, Web

## Prerequisites (all platforms)

1. **Godot 4.3+** with .NET support (the `mono` / C# build — not the standard build)
2. **.NET 8 SDK**: https://dotnet.microsoft.com/download/dotnet/8.0
3. Export templates installed in Godot:
   `Editor → Manage Export Templates → Download`

---

## Windows

### Setup
No additional tools required beyond Godot + .NET SDK.

### Export steps
1. `Project → Export → Add → Windows Desktop`
2. Set `Export Path` to `builds/windows/<GameName>.exe`
3. Click `Export Project`

The preset in `export_presets.cfg` is pre-configured. Update these fields per game:
- `application/company_name`
- `application/product_name`
- `application/copyright`

### Code signing (optional, removes SmartScreen warning)
Requires a code signing certificate (EV or OV). Set `codesign/enable=true` and point
`codesign/identity` at your `.pfx` file. Skip for early builds.

---

## Android

### One-time machine setup

**Step 1 — Android SDK + NDK**

Install via Android Studio (recommended) or command-line tools:
```bash
# Install Android Studio: https://developer.android.com/studio
# Then in Android Studio: SDK Manager → SDK Tools → NDK (Side by side) ✓
```

Default SDK path:
- Windows: `%LOCALAPPDATA%\Android\Sdk`
- Linux/Mac: `~/Android/Sdk`

Required SDK components:
- Android SDK Platform 34 (API 34)
- Android SDK Build-Tools 34
- NDK (Side by side) — any recent version
- Android Emulator (optional, for testing)

**Step 2 — .NET Android workload**
```bash
dotnet workload install android
```

**Step 3 — Java (JDK 17)**
```bash
# Windows: download from https://adoptium.net
# Ubuntu/Debian:
sudo apt install openjdk-17-jdk
```

**Step 4 — Configure Godot**

`Editor → Editor Settings → Export → Android`:
- `Android Sdk Path`: path to SDK folder
- `Java Sdk Path`: path to JDK 17 folder

**Step 5 — Debug keystore**

Godot needs a debug keystore to sign debug builds:
```bash
keytool -genkey -v \
  -keystore debug.keystore \
  -alias androiddebugkey \
  -keyalg RSA -keysize 2048 \
  -validity 10000 \
  -storepass android \
  -keypass android \
  -dname "CN=Android Debug,O=Android,C=US"
```

Save `debug.keystore` somewhere permanent (e.g. `~/.android/debug.keystore`).

In `export_presets.cfg`, set:
```
keystore/debug="<absolute path to debug.keystore>"
keystore/debug_user="androiddebugkey"
keystore/debug_password="android"
```

### Per-game setup (do this when starting a new port)

1. Update `export_presets.cfg`:
   - `package/unique_name` — reverse-domain bundle ID, e.g. `com.arcore.plumpyadventures`
   - `package/name` — display name shown on device
   - `version/code` — integer, increment each release
   - `version/name` — semver string shown to users

2. Add launcher icons:
   - `launcher_icons/main_192x192` — 192×192 PNG (app icon)
   - `launcher_icons/adaptive_foreground_432x432` — 432×432 foreground layer

### Export steps (debug APK)

1. Enable developer mode on your Android device (Settings → About → tap Build Number 7×)
2. Enable USB debugging and connect device
3. In Godot: `Project → Export → Android → Export Project`
4. Choose `.apk` format, click `Export`
5. Install via `adb install builds/android/game.apk`

### Export for Play Store (release AAB)

1. Generate a release keystore (keep it safe — losing it means you can't update your app):
   ```bash
   keytool -genkey -v \
     -keystore release.keystore \
     -alias release \
     -keyalg RSA -keysize 2048 \
     -validity 25000
   ```

2. In `export_presets.cfg`:
   ```
   export_path="builds/android/game.aab"   ; .aab not .apk
   keystore/release="<path to release.keystore>"
   keystore/release_user="release"
   keystore/release_password="<your password>"
   ```

3. Export and upload the `.aab` to Google Play Console.

### C# AOT on Android — what to watch for

Android uses **ahead-of-time (AOT) compilation** for C# — no JIT. Things that work on
desktop but break on Android:

| Pattern | Issue | Fix |
|---------|-------|-----|
| `typeof(T).GetMethod(...)` | Reflection may be trimmed | Avoid reflection; use interfaces/virtuals |
| `dynamic` keyword | Not supported | Use `object` + explicit casts |
| `Activator.CreateInstance(type)` | May be trimmed | Use `new T()` directly |
| LINQ with complex expression trees | May not compile AOT | Use simple LINQ or foreach |

ArcoreGameCore's current codebase avoids all of these — safe to export as-is.

---

## Web

### Setup
No additional tools. Web export template included with Godot.

### Export steps
1. `Project → Export → Web → Export Project`
2. Output goes to `builds/web/index.html`
3. Serve with any static file server (must be HTTPS or localhost for SharedArrayBuffer):
   ```bash
   npx serve builds/web
   # or
   python3 -m http.server 8080 --directory builds/web
   ```

### itch.io upload
1. Zip the entire `builds/web/` folder
2. Upload to itch.io as HTML game
3. Check "This file will be played in the browser"
4. Set viewport to 1920×1080 (or your game's resolution)

### COOP/COEP headers (required for threads)
If you enable threads in the export, your web server must send these headers:
```
Cross-Origin-Opener-Policy: same-origin
Cross-Origin-Embedder-Policy: require-corp
```
itch.io handles this automatically. For self-hosted, configure your server or Cloudflare Worker.

### WebPlatformAdapter
`WebPlatformAdapter` in ArcoreGameCore handles web-specific features automatically when
`OS.GetName() == "Web"`. No extra code needed — `PlatformBridge` selects it at boot.

---

## Build output structure

```
builds/
  windows/
    game.exe
  android/
    game.apk        (debug / sideload)
    game.aab        (Play Store release)
  web/
    index.html
    index.js
    index.pck
    index.wasm
```

Add `builds/` to `.gitignore` — build artifacts should not be committed.

---

## Platform support matrix

| Feature | Windows | Android | Web |
|---------|---------|---------|-----|
| C# / .NET 8 | ✓ JIT | ✓ AOT | ✓ AOT |
| SaveManager (file) | ✓ | ✓ `user://` | ✓ `user://` (IndexedDB) |
| LocalPlatformAdapter | ✓ | ✓ | – (uses WebPlatformAdapter) |
| SteamPlatformAdapter | ✓ | – | – |
| WebPlatformAdapter | – | – | ✓ auto-selected |
| GameController input | ✓ | ✓ (USB/BT) | ✓ (browser gamepad API) |
| Screen shake | ✓ | ✓ | ✓ |
| GPUParticles2D | ✓ | ✓ | ✓ |
| Audio | ✓ | ✓ | ✓ (requires user gesture) |
