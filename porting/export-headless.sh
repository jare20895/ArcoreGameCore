#!/usr/bin/env bash
# Headless Godot 4 C# export script — for CI servers and Linux machines without a display.
#
# Usage:
#   cd <project root>          (the directory containing project.godot)
#   bash ArcoreGameCore/porting/export-headless.sh [windows|linux|android|all]
#
# Requirements:
#   - Godot 4.4.1+ mono binary at GODOT_BIN (see below)
#   - .NET 8 SDK installed (via dotnet-install.sh or system package)
#   - Xvfb for the virtual display (apt install xvfb)
#   - For Android: Android SDK + NDK at ANDROID_SDK_ROOT

set -euo pipefail

# ── configuration ────────────────────────────────────────────────────────────
GODOT_VERSION="4.4.1"
GODOT_BIN="${GODOT_BIN:-$HOME/.local/bin/godot441_mono}"
DOTNET_ROOT="${DOTNET_ROOT:-$HOME/dotnet}"
ANDROID_SDK_ROOT="${ANDROID_SDK_ROOT:-$HOME/Android/Sdk}"
DISPLAY_NUM="${DISPLAY_NUM:-:2}"
TARGET="${1:-all}"
PROJECT_DIR="$(pwd)"
# ─────────────────────────────────────────────────────────────────────────────

# Colours
RED='\033[0;31m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'; NC='\033[0m'
info()  { echo -e "${GREEN}[export]${NC} $*"; }
warn()  { echo -e "${YELLOW}[export]${NC} $*"; }
error() { echo -e "${RED}[export]${NC} $*"; exit 1; }

# ── helpers ───────────────────────────────────────────────────────────────────
check_godot() {
    [[ -x "$GODOT_BIN" ]] || error "Godot binary not found at $GODOT_BIN.
  Download: https://github.com/godotengine/godot/releases/tag/${GODOT_VERSION}-stable
  Use the *mono* Linux binary (Godot_v${GODOT_VERSION}-stable_mono_linux_x86_64.zip)
  Extract and set GODOT_BIN to the path of the extracted binary."

    local ver
    ver="$("$GODOT_BIN" --version 2>/dev/null)"
    [[ "$ver" == *"mono"* ]] || error "Godot binary at $GODOT_BIN is not a mono build.
  The standard binary does NOT support C# exports.
  Download the mono version: Godot_v${GODOT_VERSION}-stable_mono_linux_x86_64.zip"
    info "Godot: $ver"
}

check_dotnet() {
    local dotnet_exe="$DOTNET_ROOT/dotnet"
    [[ -x "$dotnet_exe" ]] || error ".NET SDK not found at $DOTNET_ROOT.
  Install with: curl -L https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 8.0 --install-dir $DOTNET_ROOT"
    info ".NET: $("$dotnet_exe" --version)"
}

check_templates() {
    local template_dir="$HOME/.local/share/godot/export_templates/${GODOT_VERSION}.stable.mono"
    if [[ ! -d "$template_dir" ]]; then
        error "Mono export templates not found at $template_dir.
  IMPORTANT: You need the *mono* export templates, not the standard ones.
  Download: https://github.com/godotengine/godot/releases/download/${GODOT_VERSION}-stable/Godot_v${GODOT_VERSION}-stable_mono_export_templates.tpz
  Then extract: unzip Godot_v${GODOT_VERSION}-stable_mono_export_templates.tpz -d /tmp/templates
  Then copy:    cp /tmp/templates/templates/* $template_dir/"
    fi
    local win_tpl="$template_dir/windows_release_x86_64.exe"
    if [[ -f "$win_tpl" ]]; then
        local ver
        ver="$(DISPLAY=:99 "$win_tpl" --version 2>/dev/null || true)"
        [[ "$ver" == *"mono"* ]] || warn "Windows template may not be mono-enabled (version: $ver).
  The standard export_templates.tpz does NOT include C# support.
  Re-download using the mono_export_templates.tpz URL above."
    fi
    info "Export templates OK: $template_dir"
}

start_xvfb() {
    if ! xdpyinfo -display "$DISPLAY_NUM" >/dev/null 2>&1; then
        info "Starting Xvfb on $DISPLAY_NUM"
        Xvfb "$DISPLAY_NUM" -screen 0 1280x720x24 &
        XVFB_PID=$!
        sleep 1
    fi
    export DISPLAY="$DISPLAY_NUM"
}

build_dotnet() {
    info "Building C# project..."
    cd "$PROJECT_DIR"
    PATH="$DOTNET_ROOT:$PATH" "$DOTNET_ROOT/dotnet" build \
        PlumpyAdventures.csproj \
        -c ExportRelease \
        --no-incremental 2>&1 | tail -5
}

run_godot_export() {
    local preset="$1" output="$2"
    info "Exporting: $preset → $output"
    mkdir -p "$(dirname "$output")"
    PATH="$DOTNET_ROOT:$PATH" DISPLAY="$DISPLAY_NUM" DOTNET_ROOT="$DOTNET_ROOT" \
        "$GODOT_BIN" --headless --export-release "$preset" "$output" 2>&1 \
        | grep -v "step\|begin:\|end\|Registering\|Updating\|Scanning\|Verifying\|Loading\|Importing\|Executing\|Preparing\|Finalizing\|Creating\|Reopening\|Starting\|Initializing" \
        || true
}

# ── main ──────────────────────────────────────────────────────────────────────
cd "$PROJECT_DIR"
[[ -f "project.godot" ]] || error "No project.godot found in $PROJECT_DIR"

check_godot
check_dotnet
check_templates
start_xvfb
build_dotnet

case "$TARGET" in
    windows)
        run_godot_export "Windows Desktop" "builds/windows/PlumpyAdventures.exe"
        info "✓ Windows build: builds/windows/"
        ;;
    linux)
        run_godot_export "Linux/X11" "builds/linux/PlumpyAdventures.x86_64"
        chmod +x builds/linux/PlumpyAdventures.x86_64
        info "✓ Linux build: builds/linux/"
        ;;
    android)
        warn "Android C# export is experimental. Requires:"
        warn "  - Android SDK at $ANDROID_SDK_ROOT"
        warn "  - NDK installed: \$ANDROID_SDK_ROOT/ndk/<version>"
        warn "  - Java SDK accessible inside Godot (may need a non-Flatpak Godot)"
        run_godot_export "Android" "builds/android/PlumpyAdventures.apk"
        info "✓ Android build: builds/android/"
        ;;
    all)
        run_godot_export "Windows Desktop" "builds/windows/PlumpyAdventures.exe"
        run_godot_export "Linux/X11"       "builds/linux/PlumpyAdventures.x86_64"
        chmod +x builds/linux/PlumpyAdventures.x86_64
        info "✓ All builds done: builds/"
        ;;
    *)
        error "Unknown target '$TARGET'. Use: windows | linux | android | all"
        ;;
esac
