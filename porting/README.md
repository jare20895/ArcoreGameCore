# ArcoreGameCore — AI-Assisted Porting System

This directory contains the schema and CLI for tracking Phaser → Godot ports.
Game-specific manifests and notes live in the game's own repo under `porting/`.

## Files

```
ArcoreGameCore/porting/
  schema/
    port-manifest.schema.json    JSON schema for component inventory
    asset-manifest.schema.json   JSON schema for asset tracking
  cli/
    port-query.js                CLI query tool (Node.js, no dependencies)
  README.md                      This file

PlumpyAdventures/porting/
  port-manifest.json             Component inventory (44 components)
  asset-manifest.json            Asset inventory (24 assets)
  mapping-decisions.md           Log of non-obvious Phaser→Godot decisions
  notes/
    entity-plumpy.md             Detailed Plumpy porting brief
    entity-enemy.md              Enemy porting brief
    system-dialogue.md           Dialogue system brief
    system-crew-assist.md        Crew assist system brief
    scene-boot.md                Boot scene / SVG export process
    scene-beach-day.md           Chapter 1 pilot scene brief
    data-plumpy-art.md           SVG export pipeline (critical path)
```

## CLI quick-start

```bash
cd ArcoreGameCore/porting/cli

# Overall progress
node port-query.js status

# What to port next (dep-ordered)
node port-query.js next

# Full AI brief for one component (paste into Claude to start porting)
node port-query.js context entity/plumpy

# Show what's waiting on dependencies
node port-query.js blocked

# Mark a component done
node port-query.js set entity/plumpy done

# Filter component list
node port-query.js list --status=pending --type=entity
node port-query.js list --complexity=very-high

# Search by keyword
node port-query.js search dialogue
node port-query.js search boss

# Point at a different game's manifest
node port-query.js status --manifest=../../OtherGame/porting/port-manifest.json
```

## AI porting workflow

1. Run `node port-query.js next` to get the ordered list of what to port.
2. Run `node port-query.js context <id>` for the component you're working on.
3. Copy the context output into a new Claude conversation.
4. Read the source file at the `Full path:` listed in the context.
5. Port the component to Godot C#.
6. Run `node port-query.js set <id> done` when complete.
7. Repeat from step 1.

The `context` command outputs:
- Source file path (full, ready to `cat`)
- Godot target paths
- All dependencies with their current status
- All components that depend on this one (blast radius awareness)
- All assets required and their import status
- Phaser→Godot mapping hints specific to this component type
- Full detailed notes from `notes/<id>.md` if it exists

## Adding a new game

1. Copy `schema/port-manifest.schema.json` as reference
2. Create `<YourGame>/porting/port-manifest.json` — inventory all source files
3. Create `<YourGame>/porting/asset-manifest.json` — inventory all assets
4. Create `<YourGame>/porting/mapping-decisions.md` — log decisions as you go
5. Create `<YourGame>/porting/notes/` — add per-component briefs for complex items
6. Run the CLI with `--manifest=<path>` pointing at your game

## Status legend

| Icon | Status | Meaning |
|------|--------|---------|
| ○ | pending | Not started |
| ◑ | in-progress | Being worked on |
| ✓ | done | Ported and verified |
| ✗ | blocked | Cannot proceed, reason in `blocked_by` |
| – | skipped | Not needed in Godot |
| ⬡ | framework | Handled by ArcoreGameCore autoload/framework |
