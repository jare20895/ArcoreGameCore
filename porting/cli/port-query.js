#!/usr/bin/env node
/**
 * port-query — AI-assisted porting CLI for ArcoreGameCore
 *
 * Usage:
 *   node port-query.js <command> [options] [--manifest=<path>]
 *
 * Commands:
 *   status                    Overall progress summary
 *   list [--status=] [--type=] [--complexity=] [--priority=]
 *   next [--n=5]              Suggest next components to port (dep-ordered)
 *   show <id>                 Full details for one component
 *   deps <id>                 Dependency chain for a component
 *   blocked                   Show blocked components and reasons
 *   context <id>              AI porting brief — everything needed to port this component
 *   set <id> <status>         Update component status
 *   assets [--status=]        Asset manifest summary
 *   search <term>             Find components by keyword
 *
 * Default manifest: ../../PlumpyAdventures/porting/port-manifest.json
 * Override:         --manifest=<path>
 */

'use strict';

const fs = require('fs');
const path = require('path');

// ── Config ─────────────────────────────────────────────────────────────────

const DEFAULT_MANIFEST = path.resolve(__dirname, '../../../PlumpyAdventures/porting/port-manifest.json');
const DEFAULT_ASSETS   = path.resolve(__dirname, '../../../PlumpyAdventures/porting/asset-manifest.json');

const STATUS_ICON = {
  pending:     '○',
  'in-progress':'◑',
  done:        '✓',
  blocked:     '✗',
  skipped:     '–',
  framework:   '⬡',
};

const COMPLEXITY_COLOR = {
  'low':       '\x1b[32m',  // green
  'medium':    '\x1b[33m',  // yellow
  'high':      '\x1b[31m',  // red
  'very-high': '\x1b[35m',  // magenta
};

const RESET = '\x1b[0m';
const BOLD  = '\x1b[1m';
const DIM   = '\x1b[2m';
const CYAN  = '\x1b[36m';
const GREEN = '\x1b[32m';
const RED   = '\x1b[31m';

// ── Helpers ────────────────────────────────────────────────────────────────

function loadManifest(manifestPath) {
  if (!fs.existsSync(manifestPath)) {
    die(`Manifest not found: ${manifestPath}\nPass --manifest=<path> to specify a different location.`);
  }
  try {
    return JSON.parse(fs.readFileSync(manifestPath, 'utf8'));
  } catch (e) {
    die(`Failed to parse manifest: ${e.message}`);
  }
}

function loadAssets(assetsPath) {
  if (!fs.existsSync(assetsPath)) return null;
  try {
    return JSON.parse(fs.readFileSync(assetsPath, 'utf8'));
  } catch { return null; }
}

function die(msg) {
  console.error(`${RED}Error:${RESET} ${msg}`);
  process.exit(1);
}

function parseArgs(argv) {
  const flags = {};
  const positional = [];
  for (const arg of argv) {
    const m = arg.match(/^--([^=]+)(?:=(.*))?$/);
    if (m) {
      flags[m[1]] = m[2] !== undefined ? m[2] : true;
    } else {
      positional.push(arg);
    }
  }
  return { flags, positional };
}

function padEnd(str, len) {
  return (str + ' '.repeat(len)).slice(0, len);
}

function statusIcon(s) {
  return STATUS_ICON[s] || '?';
}

function complexityStr(c) {
  const col = COMPLEXITY_COLOR[c] || '';
  return `${col}${c}${RESET}`;
}

function readNotesFile(notesFile, manifestDir) {
  if (!notesFile) return null;
  const p = path.resolve(manifestDir, '..', notesFile);
  if (!fs.existsSync(p)) return null;
  return fs.readFileSync(p, 'utf8');
}

function topoSort(components) {
  const byId = Object.fromEntries(components.map(c => [c.id, c]));
  const visited = new Set();
  const result = [];

  function visit(id) {
    if (visited.has(id)) return;
    visited.add(id);
    const c = byId[id];
    if (!c) return;
    for (const dep of (c.dependencies || [])) visit(dep);
    result.push(c);
  }

  components.forEach(c => visit(c.id));
  return result;
}

// ── Commands ───────────────────────────────────────────────────────────────

function cmdStatus(manifest) {
  const cs = manifest.components;
  const byStatus = {};
  let totalHours = 0, doneHours = 0;

  for (const c of cs) {
    byStatus[c.status] = (byStatus[c.status] || 0) + 1;
    if (c.estimated_hours) {
      totalHours += c.estimated_hours;
      if (c.status === 'done') doneHours += c.estimated_hours;
    }
  }

  const done     = byStatus['done']        || 0;
  const fw       = byStatus['framework']   || 0;
  const skipped  = byStatus['skipped']     || 0;
  const pending  = byStatus['pending']     || 0;
  const inprog   = byStatus['in-progress'] || 0;
  const blocked  = byStatus['blocked']     || 0;
  const complete = done + fw + skipped;
  const pct = Math.round((complete / cs.length) * 100);

  console.log(`\n${BOLD}${manifest.game} — Port Status${RESET}`);
  console.log(`${'─'.repeat(48)}`);
  console.log(`  Total components:  ${cs.length}`);
  console.log(`  ${GREEN}✓ Done:${RESET}            ${done}`);
  console.log(`  ${DIM}⬡ Framework:${RESET}       ${fw}`);
  console.log(`  ${DIM}– Skipped:${RESET}         ${skipped}`);
  console.log(`  ◑ In progress:     ${inprog}`);
  console.log(`  ○ Pending:         ${pending}`);
  console.log(`  ${RED}✗ Blocked:${RESET}         ${blocked}`);
  console.log(`${'─'.repeat(48)}`);
  console.log(`  Progress:          ${pct}% (${complete}/${cs.length})`);
  console.log(`  Est. hours total:  ${totalHours.toFixed(1)}`);
  console.log(`  Est. hours remain: ${(totalHours - doneHours).toFixed(1)}\n`);

  // Breakdown by type
  const byType = {};
  for (const c of cs) {
    if (!byType[c.type]) byType[c.type] = { done: 0, total: 0 };
    byType[c.type].total++;
    if (['done','framework','skipped'].includes(c.status)) byType[c.type].done++;
  }
  console.log(`  By type:`);
  for (const [type, counts] of Object.entries(byType).sort()) {
    const bar = Math.round((counts.done / counts.total) * 10);
    const barStr = '█'.repeat(bar) + '░'.repeat(10 - bar);
    console.log(`    ${padEnd(type, 12)}  ${barStr}  ${counts.done}/${counts.total}`);
  }
  console.log();
}

function cmdList(manifest, flags) {
  let cs = [...manifest.components];
  if (flags.status)     cs = cs.filter(c => c.status === flags.status);
  if (flags.type)       cs = cs.filter(c => c.type === flags.type);
  if (flags.complexity) cs = cs.filter(c => c.complexity === flags.complexity);
  if (flags.priority)   cs = cs.filter(c => c.priority === parseInt(flags.priority));

  console.log(`\n${BOLD}${manifest.game} — Components (${cs.length})${RESET}\n`);
  console.log(`  ${padEnd('ID', 32)} ${padEnd('TYPE', 10)} ${padEnd('STATUS', 12)} ${padEnd('COMPLEXITY', 12)} HRS`);
  console.log(`  ${'─'.repeat(76)}`);

  for (const c of cs) {
    const icon = statusIcon(c.status);
    const hrs = c.estimated_hours != null ? c.estimated_hours.toFixed(1) : '—';
    console.log(
      `  ${icon} ${padEnd(c.id, 30)} ${padEnd(c.type, 10)} ${padEnd(c.status, 12)} ${padEnd(c.complexity, 12)} ${hrs}`
    );
  }
  console.log();
}

function cmdNext(manifest, flags) {
  const n = parseInt(flags.n || '5');
  const portable = manifest.components.filter(c =>
    c.status === 'pending' || c.status === 'in-progress'
  );

  const byId = Object.fromEntries(manifest.components.map(c => [c.id, c]));

  function depsAllDone(component) {
    return (component.dependencies || []).every(depId => {
      const dep = byId[depId];
      return !dep || ['done', 'framework', 'skipped'].includes(dep.status);
    });
  }

  const ready = portable.filter(depsAllDone);
  const sorted = ready.sort((a, b) => (a.priority || 99) - (b.priority || 99));
  const picks = sorted.slice(0, n);

  console.log(`\n${BOLD}${manifest.game} — Next ${n} to Port${RESET}\n`);
  if (picks.length === 0) {
    console.log('  Nothing ready — check blocked items.\n');
    return;
  }

  for (let i = 0; i < picks.length; i++) {
    const c = picks[i];
    console.log(`  ${BOLD}${i + 1}. ${c.id}${RESET}  ${DIM}(${c.type}, ${c.complexity})${RESET}`);
    console.log(`     ${c.label}`);
    if (c.estimated_hours) console.log(`     Est: ${c.estimated_hours}h  |  Source: ${c.source_file} (${c.source_lines} lines)`);
    if (c.notes) console.log(`     ${DIM}${c.notes}${RESET}`);
    console.log();
  }
}

function cmdShow(manifest, id) {
  const c = manifest.components.find(x => x.id === id);
  if (!c) die(`Component not found: ${id}`);

  const byId = Object.fromEntries(manifest.components.map(x => [x.id, x]));

  console.log(`\n${BOLD}${CYAN}${c.id}${RESET}  ${statusIcon(c.status)} ${c.status}`);
  console.log(`${'─'.repeat(60)}`);
  console.log(`  Label:       ${c.label}`);
  console.log(`  Type:        ${c.type}`);
  console.log(`  Complexity:  ${complexityStr(c.complexity)}`);
  console.log(`  Priority:    ${c.priority}`);
  console.log(`  Est. hours:  ${c.estimated_hours ?? '—'}`);
  console.log();
  console.log(`  Source:      ${c.source_file} (${c.source_lines} lines)`);
  if (c.godot_scene)  console.log(`  Godot scene: ${c.godot_scene}`);
  if (c.godot_script) console.log(`  Godot script:${c.godot_script}`);
  console.log();

  if (c.notes) {
    console.log(`  Notes:`);
    console.log(`    ${c.notes}`);
    console.log();
  }

  if (c.blocked_by) {
    console.log(`  ${RED}Blocked by: ${c.blocked_by}${RESET}\n`);
  }

  if (c.dependencies && c.dependencies.length > 0) {
    console.log(`  Dependencies:`);
    for (const depId of c.dependencies) {
      const dep = byId[depId];
      const icon = dep ? statusIcon(dep.status) : '?';
      const label = dep ? dep.label : '(not found)';
      console.log(`    ${icon} ${depId}  ${DIM}${label}${RESET}`);
    }
    console.log();
  }

  if (c.notes_file) {
    console.log(`  Detailed notes: ${c.notes_file}`);
    console.log(`  Run: ${DIM}node port-query.js context ${c.id}${RESET} to see full AI brief\n`);
  }
}

function cmdDeps(manifest, id) {
  const byId = Object.fromEntries(manifest.components.map(c => [c.id, c]));
  const visited = new Set();

  function print(cid, depth) {
    const c = byId[cid];
    const indent = '  ' + '  '.repeat(depth);
    const icon = c ? statusIcon(c.status) : '?';
    const label = c ? ` ${DIM}${c.label}${RESET}` : '';
    console.log(`${indent}${icon} ${cid}${label}`);
    if (!c || visited.has(cid)) return;
    visited.add(cid);
    for (const dep of (c.dependencies || [])) print(dep, depth + 1);
  }

  console.log(`\n${BOLD}Dependency tree: ${id}${RESET}\n`);
  print(id, 0);
  console.log();
}

function cmdBlocked(manifest) {
  const blocked = manifest.components.filter(c => c.status === 'blocked');
  const byId = Object.fromEntries(manifest.components.map(c => [c.id, c]));

  // Also find implicitly blocked: pending but deps not done
  const implicitlyBlocked = manifest.components.filter(c => {
    if (c.status !== 'pending') return false;
    return (c.dependencies || []).some(depId => {
      const dep = byId[depId];
      return dep && !['done', 'framework', 'skipped'].includes(dep.status);
    });
  });

  console.log(`\n${BOLD}${manifest.game} — Blocked Components${RESET}\n`);

  if (blocked.length === 0 && implicitlyBlocked.length === 0) {
    console.log(`  ${GREEN}No blocked components.${RESET}\n`);
    return;
  }

  if (blocked.length > 0) {
    console.log(`  ${RED}Explicitly blocked:${RESET}`);
    for (const c of blocked) {
      console.log(`    ✗ ${c.id}  —  ${c.blocked_by || 'no reason given'}`);
    }
    console.log();
  }

  if (implicitlyBlocked.length > 0) {
    console.log(`  ${BOLD}Waiting on dependencies:${RESET}`);
    for (const c of implicitlyBlocked) {
      const unmet = (c.dependencies || []).filter(depId => {
        const dep = byId[depId];
        return dep && !['done', 'framework', 'skipped'].includes(dep.status);
      });
      console.log(`    ○ ${c.id}`);
      for (const u of unmet) {
        const dep = byId[u];
        console.log(`       waiting on: ${statusIcon(dep?.status || '?')} ${u}`);
      }
    }
    console.log();
  }
}

function cmdContext(manifest, id, assetsManifest) {
  const c = manifest.components.find(x => x.id === id);
  if (!c) die(`Component not found: ${id}`);

  const byId = Object.fromEntries(manifest.components.map(x => [x.id, x]));
  const sourceRoot = path.dirname(manifestPath);

  const sep = '═'.repeat(60);
  console.log(`\n${BOLD}${sep}`);
  console.log(`  AI PORTING CONTEXT: ${id}`);
  console.log(`${sep}${RESET}\n`);

  console.log(`${BOLD}SOURCE${RESET}`);
  console.log(`  File:        ${manifest.source_root}${c.source_file}`);
  console.log(`  Lines:       ${c.source_lines}`);
  console.log(`  Full path:   ${path.resolve(sourceRoot, '..', manifest.source_root, c.source_file)}`);
  console.log();

  console.log(`${BOLD}GODOT TARGET${RESET}`);
  if (c.godot_scene)  console.log(`  Scene:       ${c.godot_scene}`);
  if (c.godot_script) console.log(`  Script:      ${c.godot_script}`);
  if (!c.godot_scene && !c.godot_script) console.log(`  (no Godot target — data conversion or skip)`);
  console.log();

  console.log(`${BOLD}STATUS${RESET}        ${statusIcon(c.status)} ${c.status}`);
  console.log(`${BOLD}COMPLEXITY${RESET}    ${c.complexity}`);
  console.log(`${BOLD}PRIORITY${RESET}      ${c.priority}`);
  console.log(`${BOLD}EST. HOURS${RESET}    ${c.estimated_hours ?? '—'}`);
  console.log();

  if (c.notes) {
    console.log(`${BOLD}SUMMARY${RESET}`);
    console.log(`  ${c.notes}`);
    console.log();
  }

  if (c.blocked_by) {
    console.log(`${RED}${BOLD}BLOCKED:${RESET} ${c.blocked_by}\n`);
  }

  if (c.dependencies && c.dependencies.length > 0) {
    console.log(`${BOLD}DEPENDENCIES${RESET}`);
    for (const depId of c.dependencies) {
      const dep = byId[depId];
      if (!dep) { console.log(`  ? ${depId}  (not in manifest)`); continue; }
      const icon = statusIcon(dep.status);
      const ready = ['done', 'framework', 'skipped'].includes(dep.status);
      const col = ready ? GREEN : RED;
      console.log(`  ${col}${icon} ${depId}${RESET}  ${DIM}${dep.label} (${dep.status})${RESET}`);
      if (!ready) {
        console.log(`    ${RED}⚠ Not yet done — port this first${RESET}`);
      }
    }
    console.log();
  }

  // Reverse deps: who uses this component?
  const usedBy = manifest.components.filter(x =>
    (x.dependencies || []).includes(id)
  );
  if (usedBy.length > 0) {
    console.log(`${BOLD}USED BY${RESET}`);
    for (const u of usedBy) console.log(`  ${statusIcon(u.status)} ${u.id}  ${DIM}${u.label}${RESET}`);
    console.log();
  }

  // Relevant assets
  if (assetsManifest) {
    const relevantAssets = assetsManifest.assets.filter(a =>
      (a.used_by || []).includes(id)
    );
    if (relevantAssets.length > 0) {
      console.log(`${BOLD}ASSETS NEEDED${RESET}`);
      for (const a of relevantAssets) {
        const ready = a.status === 'done' || a.status === 'imported';
        const col = ready ? GREEN : RED;
        console.log(`  ${col}${statusIcon(a.status)} ${a.id}${RESET}  →  ${a.godot_path || '(no path)'}`);
      }
      console.log();
    }
  }

  // Phaser→Godot mapping hints for this component type
  console.log(`${BOLD}MAPPING HINTS${RESET}`);
  const hints = getMappingHints(c.type, c.source_file);
  for (const h of hints) console.log(`  ${DIM}•${RESET} ${h}`);
  console.log();

  // Detailed notes file
  if (c.notes_file) {
    const notesContent = readNotesFile(c.notes_file, path.dirname(manifestPath));
    if (notesContent) {
      console.log(`${BOLD}DETAILED PORTING NOTES${RESET}`);
      console.log(`${'─'.repeat(60)}`);
      console.log(notesContent);
    } else {
      console.log(`${BOLD}DETAILED NOTES${RESET}  ${c.notes_file}  ${DIM}(file not found)${RESET}\n`);
    }
  }

  console.log(`${DIM}${'─'.repeat(60)}`);
  console.log(`Reference: ArcoreGameCore/docs/PHASER_TO_GODOT.md`);
  console.log(`Decisions: PlumpyAdventures/porting/mapping-decisions.md${RESET}\n`);
}

function getMappingHints(type, sourceFile) {
  const common = [
    'Phaser Scene lifecycle → _Ready() / _Process() / _PhysicsProcess()',
    'scene.time.delayedCall(ms) → await ToSignal(CreateTimer(s), "timeout")',
    'scene.tweens.add({...}) → CreateTween().TweenProperty(...)',
    'this.scene.input.keyboard → Input.IsActionPressed() / InputManager',
  ];
  const typeHints = {
    entity: [
      'Phaser.Physics.Arcade.Sprite → CharacterBody2D',
      'body.blocked.down → IsOnFloor() (call after MoveAndSlide)',
      'setVelocityX/Y → Velocity = new Vector2(...)',
      'setGravityY() per-body → manual += gravity * delta each frame',
      'setFlipX → FlipH (on Sprite2D) or Scale.X *= -1',
    ],
    scene: [
      'Phaser StaticGroup → StaticBody2D children under a Node2D parent',
      'physics.add.overlap → Area2D + BodyEntered signal',
      'cameras.main.startFollow → Camera2D child of player',
      'cameras.main.setBounds → Camera2D Limit properties',
      'scene.add.particles → GPUParticles2D with OneShot=true',
      'triggeredIds Set → HashSet<string> _triggeredIds on scene script',
    ],
    boss: [
      'BossPhase string union → C# enum BossPhase',
      'Phase timer countdown → _phaseTimer float, reset on phase enter',
      'HP bar Rectangle.setDisplaySize → TextureProgressBar or ColorRect width',
      'Debris platforms per phase → List<Node> _phaseObjects, QueueFree on exit',
    ],
    system: [
      'keyboard.on("keydown", fn) → _UnhandledInput override on Node',
      'Container with setScrollFactor(0) → CanvasLayer child nodes',
      'scene.add.rectangle(...) → ColorRect node',
    ],
    data: [
      'TypeScript const object → C# static class or Godot Resource (.tres)',
      'Array of positions → Marker2D nodes placed in scene editor',
    ],
  };
  return [...(typeHints[type] || []), ...common];
}

function cmdSet(manifest, manifestPath, id, status) {
  const validStatuses = ['pending', 'in-progress', 'done', 'blocked', 'skipped', 'framework'];
  if (!validStatuses.includes(status)) {
    die(`Invalid status "${status}". Valid: ${validStatuses.join(', ')}`);
  }

  const c = manifest.components.find(x => x.id === id);
  if (!c) die(`Component not found: ${id}`);

  const oldStatus = c.status;
  c.status = status;
  c.last_updated = new Date().toISOString().split('T')[0];
  manifest.last_updated = c.last_updated;

  fs.writeFileSync(manifestPath, JSON.stringify(manifest, null, 2) + '\n');
  console.log(`\n  ${statusIcon(oldStatus)} ${id}  →  ${statusIcon(status)} ${status}  ${DIM}(saved)${RESET}\n`);
}

function cmdAssets(assetsManifest, flags) {
  if (!assetsManifest) {
    console.log('\n  Asset manifest not found.\n');
    return;
  }

  let assets = [...assetsManifest.assets];
  if (flags.status) assets = assets.filter(a => a.status === flags.status);
  if (flags.type)   assets = assets.filter(a => a.type === flags.type);

  const byStatus = {};
  for (const a of assetsManifest.assets) byStatus[a.status] = (byStatus[a.status] || 0) + 1;

  console.log(`\n${BOLD}${assetsManifest.game} — Assets${RESET}\n`);
  console.log(`  Total: ${assetsManifest.assets.length}  |  ` +
    Object.entries(byStatus).map(([s, n]) => `${statusIcon(s)} ${s}: ${n}`).join('  '));
  console.log();

  console.log(`  ${padEnd('ID', 28)} ${padEnd('TYPE', 12)} ${padEnd('STATUS', 10)} GODOT PATH`);
  console.log(`  ${'─'.repeat(80)}`);
  for (const a of assets) {
    console.log(`  ${padEnd(a.id, 28)} ${padEnd(a.type, 12)} ${padEnd(a.status, 10)} ${a.godot_path || '—'}`);
  }
  console.log();
}

function cmdSearch(manifest, term) {
  const t = term.toLowerCase();
  const matches = manifest.components.filter(c =>
    c.id.includes(t) ||
    (c.label || '').toLowerCase().includes(t) ||
    (c.notes || '').toLowerCase().includes(t) ||
    (c.source_file || '').toLowerCase().includes(t)
  );

  console.log(`\n${BOLD}Search: "${term}" — ${matches.length} results${RESET}\n`);
  for (const c of matches) {
    console.log(`  ${statusIcon(c.status)} ${CYAN}${c.id}${RESET}  ${DIM}${c.label}${RESET}`);
    if (c.notes && c.notes.toLowerCase().includes(t)) {
      const idx = c.notes.toLowerCase().indexOf(t);
      const snippet = c.notes.slice(Math.max(0, idx - 30), idx + 60);
      console.log(`    ${DIM}...${snippet}...${RESET}`);
    }
  }
  console.log();
}

function printHelp() {
  console.log(`
${BOLD}port-query${RESET} — AI-assisted porting CLI for ArcoreGameCore

${BOLD}Commands:${RESET}
  status                     Overall progress summary with breakdown by type
  list [--status=] [--type=] [--complexity=] [--priority=]
                             Filtered component list
  next [--n=5]               Next N components ready to port (dep-ordered)
  show <id>                  Full details for one component
  deps <id>                  Dependency tree for a component
  blocked                    Show blocked and dependency-waiting components
  context <id>               ${BOLD}AI porting brief${RESET} — all info needed to port this component
  set <id> <status>          Update a component's status in the manifest
  assets [--status=] [--type=]  Asset manifest summary
  search <term>              Find components by keyword

${BOLD}Status values:${RESET}
  pending  in-progress  done  blocked  skipped  framework

${BOLD}Options:${RESET}
  --manifest=<path>          Path to port-manifest.json (default: PlumpyAdventures)
  --assets=<path>            Path to asset-manifest.json

${BOLD}Examples:${RESET}
  node port-query.js status
  node port-query.js next --n=3
  node port-query.js context entity/plumpy
  node port-query.js list --status=pending --type=entity
  node port-query.js set entity/plumpy in-progress
  node port-query.js blocked
`);
}

// ── Entry point ────────────────────────────────────────────────────────────

const { flags, positional } = parseArgs(process.argv.slice(2));
const command = positional[0];

const manifestPath  = flags.manifest  ? path.resolve(flags.manifest)  : DEFAULT_MANIFEST;
const assetsPath    = flags.assets    ? path.resolve(flags.assets)     : DEFAULT_ASSETS;

if (!command || command === 'help' || flags.help) {
  printHelp();
  process.exit(0);
}

const manifest      = loadManifest(manifestPath);
const assetsManifest = loadAssets(assetsPath);

switch (command) {
  case 'status':
    cmdStatus(manifest);
    break;
  case 'list':
    cmdList(manifest, flags);
    break;
  case 'next':
    cmdNext(manifest, flags);
    break;
  case 'show':
    if (!positional[1]) die('Usage: show <id>');
    cmdShow(manifest, positional[1]);
    break;
  case 'deps':
    if (!positional[1]) die('Usage: deps <id>');
    cmdDeps(manifest, positional[1]);
    break;
  case 'blocked':
    cmdBlocked(manifest);
    break;
  case 'context':
    if (!positional[1]) die('Usage: context <id>');
    cmdContext(manifest, positional[1], assetsManifest);
    break;
  case 'set':
    if (!positional[1] || !positional[2]) die('Usage: set <id> <status>');
    cmdSet(manifest, manifestPath, positional[1], positional[2]);
    break;
  case 'assets':
    cmdAssets(assetsManifest, flags);
    break;
  case 'search':
    if (!positional[1]) die('Usage: search <term>');
    cmdSearch(manifest, positional[1]);
    break;
  default:
    die(`Unknown command: ${command}\nRun "node port-query.js help" for usage.`);
}
