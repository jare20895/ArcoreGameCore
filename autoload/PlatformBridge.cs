using Godot;

namespace ArcoreGameCore;

/// <summary>
/// Abstract platform bridge. Swap implementations at boot to target Steam, Web, local, or debug.
/// </summary>
public partial class PlatformBridge : Node
{
    public static PlatformBridge Instance { get; private set; } = null!;

    private IPlatformAdapter _adapter = new LocalPlatformAdapter();

    public override void _Ready()
    {
        Instance = this;
        _adapter = ResolveAdapter();
        _adapter.Initialize();
        GD.Print($"[PlatformBridge] Using adapter: {_adapter.GetType().Name}");
    }

    // --- Public API (game code calls these, never the adapter directly) ---

    public void UnlockAchievement(string id) => _adapter.UnlockAchievement(id);
    public void SetStat(string id, int value) => _adapter.SetStat(id, value);
    public void SaveCloudFile(string path, byte[] data) => _adapter.SaveCloudFile(path, data);
    public void SetRichPresence(string status) => _adapter.SetRichPresence(status);
    public bool IsAchievementUnlocked(string id) => _adapter.IsAchievementUnlocked(id);
    public void ShowLeaderboard(string boardId) => _adapter.ShowLeaderboard(boardId);
    public void SubmitScore(string boardId, long score) => _adapter.SubmitScore(boardId, score);

    private static IPlatformAdapter ResolveAdapter()
    {
        if (OS.IsDebugBuild() && OS.HasEnvironment("ARCORE_DEBUG_PLATFORM"))
            return new DebugPlatformAdapter();

        // Steam detection — requires GodotSteam or Steamworks plugin
        if (ClassDB.ClassExists("Steam"))
            return new SteamPlatformAdapter();

        if (OS.GetName() == "Web")
            return new WebPlatformAdapter();

        return new LocalPlatformAdapter();
    }
}

/// <summary>Contract all platform adapters must fulfil.</summary>
public interface IPlatformAdapter
{
    void Initialize();
    void UnlockAchievement(string id);
    void SetStat(string id, int value);
    bool IsAchievementUnlocked(string id);
    void SaveCloudFile(string path, byte[] data);
    void SetRichPresence(string status);
    void ShowLeaderboard(string boardId);
    void SubmitScore(string boardId, long score);
}
