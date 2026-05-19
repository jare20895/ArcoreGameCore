using Godot;
using Godot.Collections;

namespace ArcoreGameCore;

/// <summary>
/// Offline / desktop adapter. Persists achievements and stats locally.
/// </summary>
public class LocalPlatformAdapter : IPlatformAdapter
{
    private const string AchievementFile = "user://achievements.json";
    private Dictionary _unlocked = new();

    public void Initialize()
    {
        if (FileAccess.FileExists(AchievementFile))
        {
            using var f = FileAccess.Open(AchievementFile, FileAccess.ModeFlags.Read);
            var json = new Json();
            if (json.Parse(f.GetAsText()) == Error.Ok)
                _unlocked = json.Data.AsGodotDictionary();
        }
    }

    public void UnlockAchievement(string id)
    {
        if (_unlocked.ContainsKey(id)) return;
        _unlocked[id] = true;
        Persist();
        GD.Print($"[Achievement] Unlocked: {id}");
    }

    public bool IsAchievementUnlocked(string id) => _unlocked.ContainsKey(id);

    public void SetStat(string id, int value)
    {
        // Stored locally alongside achievements for simplicity
        _unlocked[$"stat_{id}"] = value;
        Persist();
    }

    public void SaveCloudFile(string path, byte[] data) { /* no-op for local */ }

    public void SetRichPresence(string status) { /* no-op */ }

    public void ShowLeaderboard(string boardId) { /* no-op */ }

    public void SubmitScore(string boardId, long score) { /* no-op */ }

    private void Persist()
    {
        using var f = FileAccess.Open(AchievementFile, FileAccess.ModeFlags.Write);
        f.StoreString(Json.Stringify(_unlocked));
    }
}
