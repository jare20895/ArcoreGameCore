using Godot;

namespace ArcoreGameCore;

/// <summary>
/// Scene navigation with optional transition animations. Replaces direct GetTree().ChangeSceneToFile calls.
/// </summary>
public partial class SceneRouter : Node
{
    public static SceneRouter Instance { get; private set; } = null!;

    [Signal] public delegate void SceneChangeStartedEventHandler(string toScene);
    [Signal] public delegate void SceneChangeCompletedEventHandler(string sceneName);

    // Registered scene paths — populate these for your game
    public static class Scenes
    {
        public const string Boot = "res://scenes/boot/Boot.tscn";
        public const string MainMenu = "res://scenes/menus/MainMenu.tscn";
        public const string PauseMenu = "res://scenes/menus/PauseMenu.tscn";
        public const string HUD = "res://scenes/ui/HUD.tscn";
    }

    private bool _transitioning;

    public override void _Ready()
    {
        Instance = this;
    }

    /// <summary>Change scene, optionally running a transition overlay first.</summary>
    public void GoTo(string scenePath, bool useTransition = true)
    {
        if (_transitioning) return;

        EmitSignal(SignalName.SceneChangeStarted, scenePath);

        if (useTransition && ScreenTransition.Instance != null)
        {
            _transitioning = true;
            ScreenTransition.Instance.PlayOutro(() => DoSceneChange(scenePath));
        }
        else
        {
            DoSceneChange(scenePath);
        }
    }

    public void Reload() => GoTo(GetTree().CurrentScene?.SceneFilePath ?? Scenes.Boot);

    private void DoSceneChange(string path)
    {
        _transitioning = false;
        GetTree().ChangeSceneToFile(path);
        EmitSignal(SignalName.SceneChangeCompleted, path);
    }
}
