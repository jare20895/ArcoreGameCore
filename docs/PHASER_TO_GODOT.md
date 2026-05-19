# Phaser → Godot 4 Mapping Reference

Use this guide when porting PlumpyAdventures (and future Arcore games) from Phaser 3 to Godot 4 C#.

---

## Core lifecycle

| Phaser | Godot (C#) | Notes |
|--------|-----------|-------|
| `preload()` | Boot scene + `ResourceLoader` | Load assets in a dedicated Boot scene before the main menu |
| `create()` | `_Ready()` | Called once after node enters the tree |
| `update(time, delta)` | `_Process(double delta)` | Per-frame logic |
| — | `_PhysicsProcess(double delta)` | Fixed-rate physics logic (replaces Arcade physics step) |
| `destroy()` | `QueueFree()` | Defers removal until end of frame |

## Scenes

| Phaser | Godot | Notes |
|--------|-------|-------|
| `Scene` class | `.tscn` file + root `Node` | Scene tree replaces Phaser's scene stack |
| `this.scene.start('Key')` | `SceneRouter.Instance.GoTo(path)` | Use SceneRouter for transitions |
| `this.scene.launch('Overlay')` | `AddChild(scene)` or separate CanvasLayer | Overlapping scenes = child nodes or CanvasLayers |
| `this.scene.pause()` | `GameState.Instance.SetPaused(true)` | Sets `GetTree().Paused` |

## Display & sprites

| Phaser | Godot | Notes |
|--------|-------|-------|
| `this.add.image(x, y, key)` | `Sprite2D` node | Static image |
| `this.add.sprite(x, y, key)` | `AnimatedSprite2D` node | Animated sprite with `SpriteFrames` |
| `sprite.setFlipX(true)` | `sprite.FlipH = true` | |
| `sprite.setScale(2)` | `sprite.Scale = new Vector2(2, 2)` | |
| `sprite.setAlpha(0.5)` | `sprite.Modulate = new Color(1,1,1,0.5f)` | |
| `sprite.setDepth(n)` | `node.ZIndex = n` | |
| `this.add.tileSprite(...)` | `ParallaxBackground.cs` (custom) | See `scripts/visual/ParallaxBackground.cs` |

## Text

| Phaser | Godot | Notes |
|--------|-------|-------|
| `this.add.text(x, y, str, style)` | `Label` node | Use theme overrides for font/size |
| `text.setText(str)` | `label.Text = str` | |
| `text.setStyle({color})` | `label.Modulate = color` | |
| `this.add.bitmapText(...)` | `Label` with bitmap font | |

## Physics & movement

| Phaser | Godot | Notes |
|--------|-------|-------|
| Arcade physics body | `CharacterBody2D` | Kinematic; use `MoveAndSlide()` |
| Static arcade body | `StaticBody2D` | Immovable collidable |
| Overlap / trigger | `Area2D` + `BodyEntered` signal | |
| `body.setVelocityX(v)` | `body.Velocity = new Vector2(v, body.Velocity.Y)` | |
| `body.setGravityY(g)` | Use `ProjectSettings` gravity or add manually in `_PhysicsProcess` | |
| Tilemap collision | `TileMapLayer` with collision shapes | Set collision in TileSet editor |

## Camera

| Phaser | Godot | Notes |
|--------|-------|-------|
| `this.cameras.main` | `Camera2D` node | |
| `camera.startFollow(sprite)` | `Camera2D` as child of player, or `RemoteTransform2D` | |
| `camera.setBounds(...)` | `Camera2D.LimitLeft/Right/Top/Bottom` | |
| Camera shake | `CameraShake.cs` (custom) | See `scripts/visual/CameraShake.cs` |
| Camera zoom | `camera.Zoom = new Vector2(2, 2)` | |

## Input

| Phaser | Godot | Notes |
|--------|-------|-------|
| `this.input.keyboard.addKey(...)` | `InputMap` + `Input.IsActionPressed()` | Define actions in Project Settings → Input Map |
| `cursors.left.isDown` | `InputManager.Instance.IsActionPressed(ActionMoveLeft)` | |
| `Phaser.Input.Keyboard.JustDown(key)` | `Input.IsActionJustPressed(action)` | |
| `this.input.gamepad` | `InputManager.IsGamepad` + same action names | |

## Tweens & animation

| Phaser | Godot | Notes |
|--------|-------|-------|
| `this.tweens.add({...})` | `CreateTween()` + `.TweenProperty(...)` | |
| `tween.to({alpha:0}, 300)` | `.TweenProperty(node, "modulate:a", 0f, 0.3f)` | |
| `this.time.delayedCall(ms, fn)` | `await ToSignal(GetTree().CreateTimer(s), "timeout")` | Or `Tween.TweenCallback` |
| `AnimationPlayer` (spine) | `AnimationPlayer` node | Godot's built-in animation system |
| Spritesheet animation | `AnimatedSprite2D` with `SpriteFrames` resource | |

## Audio

| Phaser | Godot | Notes |
|--------|-------|-------|
| `this.sound.add(key)` | `AudioStreamPlayer` node | |
| `sound.play()` | `AudioManager.Instance.PlaySfx(stream)` | Pooled via AudioManager |
| `this.sound.play('bgm', {loop:true})` | `AudioManager.Instance.PlayBgm(stream)` | |
| `sound.setVolume(v)` | `AudioManager.Instance.SetBusVolume(bus, db)` | |

## Tilemaps

| Phaser | Godot | Notes |
|--------|-------|-------|
| `this.make.tilemap({key})` | `TileMapLayer` node | |
| Tileset image | `TileSet` resource | Import PNG as TileSet in editor |
| `tilemap.createLayer(...)` | One `TileMapLayer` per layer | Godot 4.3+ uses TileMapLayer |
| Tilemap objects (Tiled) | `TileMap` custom data / `Marker2D` nodes | Import Tiled maps via plugin |

## Game objects / groups

| Phaser | Godot | Notes |
|--------|-------|-------|
| `this.add.group()` | `Node2D` parent + `GetChildren()` | Groups are just nodes |
| `group.getFirstDead()` | Object pool pattern (custom) | |
| `this.physics.add.group()` | Node parent for `CharacterBody2D` children | |

## Persistence

| Phaser | Godot | Notes |
|--------|-------|-------|
| `localStorage.setItem(k, v)` | `SaveManager.Instance.Save(slot, data)` | |
| `localStorage.getItem(k)` | `SaveManager.Instance.Load(slot)` | |

## Events / signals

| Phaser | Godot | Notes |
|--------|-------|-------|
| `emitter.on('event', fn)` | `node.SignalName += Handler` | C# event subscription |
| `emitter.emit('event', data)` | `EmitSignal(SignalName.EventName, data)` | Declare with `[Signal]` attribute |
| `emitter.once('event', fn)` | Connect with `ConnectFlags.OneShot` | |

---

## Patterns that don't map 1:1

### Phaser Scene registry (`this.registry`)
Use `GameState` autoload instead. It's a global singleton accessible from any script.

### Phaser textures / atlas keys
In Godot, assets are loaded by file path via `GD.Load<T>()` or assigned in the editor.
No string key registry needed — the resource cache handles deduplication.

### Phaser `setInteractive()` / pointer events
Use `Area2D` with `InputPickable = true` and connect `InputEvent` signal,
or use `Control` nodes for UI-style input.

### Phaser physics groups with callbacks
Use `Area2D.BodyEntered` / `Area2D.AreaEntered` signals.
For layer-based filtering, set `CollisionLayer` and `CollisionMask` bitmasks.
