using Godot;

namespace ArcoreGameCore;

/// <summary>
/// Global game state singleton. Tracks current scene, session data, and game flags.
/// </summary>
public partial class GameState : Node
{
    public static GameState Instance { get; private set; } = null!;

    [Signal] public delegate void GamePausedEventHandler(bool paused);
    [Signal] public delegate void ScoreChangedEventHandler(int newScore);
    [Signal] public delegate void LivesChangedEventHandler(int newLives);

    public bool IsPaused { get; private set; }
    public int Score { get; private set; }
    public int Lives { get; private set; } = 3;
    public int Level { get; private set; } = 1;
    public string CurrentChapter { get; set; } = string.Empty;
    public bool IsDebugMode { get; private set; }

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
        IsDebugMode = OS.IsDebugBuild();
    }

    public void AddScore(int amount)
    {
        Score += amount;
        EmitSignal(SignalName.ScoreChanged, Score);
    }

    public void SetLives(int count)
    {
        Lives = Mathf.Max(0, count);
        EmitSignal(SignalName.LivesChanged, Lives);
    }

    public void LoseLife()
    {
        SetLives(Lives - 1);
    }

    public void SetPaused(bool paused)
    {
        IsPaused = paused;
        GetTree().Paused = paused;
        EmitSignal(SignalName.GamePaused, paused);
    }

    public void Reset()
    {
        Score = 0;
        Lives = 3;
        Level = 1;
        CurrentChapter = string.Empty;
        SetPaused(false);
    }
}
