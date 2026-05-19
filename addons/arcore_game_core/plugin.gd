@tool
extends EditorPlugin

func _enter_tree() -> void:
	add_autoload_singleton("GameState", "res://autoload/GameState.cs")
	add_autoload_singleton("SaveManager", "res://autoload/SaveManager.cs")
	add_autoload_singleton("AudioManager", "res://autoload/AudioManager.cs")
	add_autoload_singleton("InputManager", "res://autoload/InputManager.cs")
	add_autoload_singleton("PlatformBridge", "res://autoload/PlatformBridge.cs")
	add_autoload_singleton("SceneRouter", "res://autoload/SceneRouter.cs")

func _exit_tree() -> void:
	remove_autoload_singleton("GameState")
	remove_autoload_singleton("SaveManager")
	remove_autoload_singleton("AudioManager")
	remove_autoload_singleton("InputManager")
	remove_autoload_singleton("PlatformBridge")
	remove_autoload_singleton("SceneRouter")
