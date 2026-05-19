using Godot;
using System.Collections.Generic;

namespace ArcoreGameCore;

/// <summary>
/// Debug adapter. Logs all calls and simulates instant success for every platform operation.
/// Activate by setting env var ARCORE_DEBUG_PLATFORM=1 in a debug build.
/// </summary>
public class DebugPlatformAdapter : IPlatformAdapter
{
    private readonly HashSet<string> _achievements = new();
    private readonly Dictionary<string, int> _stats = new();

    public void Initialize() => GD.Print("[PlatformBridge] DEBUG adapter initialised.");

    public void UnlockAchievement(string id)
    {
        _achievements.Add(id);
        GD.Print($"[DEBUG Platform] UnlockAchievement: {id}");
    }

    public bool IsAchievementUnlocked(string id) => _achievements.Contains(id);

    public void SetStat(string id, int value)
    {
        _stats[id] = value;
        GD.Print($"[DEBUG Platform] SetStat: {id} = {value}");
    }

    public void SaveCloudFile(string path, byte[] data)
        => GD.Print($"[DEBUG Platform] SaveCloudFile: {path} ({data.Length} bytes)");

    public void SetRichPresence(string status)
        => GD.Print($"[DEBUG Platform] RichPresence: {status}");

    public void ShowLeaderboard(string boardId)
        => GD.Print($"[DEBUG Platform] ShowLeaderboard: {boardId}");

    public void SubmitScore(string boardId, long score)
        => GD.Print($"[DEBUG Platform] SubmitScore: {boardId} = {score}");
}
