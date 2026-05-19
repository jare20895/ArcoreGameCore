using Godot;

namespace ArcoreGameCore;

/// <summary>
/// Steam adapter. Requires GodotSteam plugin (https://godotsteam.com).
/// Set app_id in steam_appid.txt before shipping.
/// </summary>
public class SteamPlatformAdapter : IPlatformAdapter
{
    public void Initialize()
    {
        // GodotSteam auto-initialises via its autoload, but we can check here.
        // var steam = Engine.GetSingleton("Steam");
        // if (!steam.Call("isSteamRunning").AsBool())
        //     GD.PushWarning("[Steam] Steam is not running.");
        GD.Print("[PlatformBridge] Steam adapter initialised (stub).");
    }

    public void UnlockAchievement(string id)
    {
        // Engine.GetSingleton("Steam").Call("setAchievement", id);
        GD.Print($"[Steam] UnlockAchievement: {id}");
    }

    public bool IsAchievementUnlocked(string id)
    {
        // return Engine.GetSingleton("Steam").Call("getAchievement", id).AsBool();
        return false;
    }

    public void SetStat(string id, int value)
    {
        // Engine.GetSingleton("Steam").Call("setStatInt", id, value);
        GD.Print($"[Steam] SetStat: {id} = {value}");
    }

    public void SaveCloudFile(string path, byte[] data)
    {
        // Engine.GetSingleton("Steam").Call("fileWrite", path, data);
    }

    public void SetRichPresence(string status)
    {
        // Engine.GetSingleton("Steam").Call("setRichPresence", "status", status);
    }

    public void ShowLeaderboard(string boardId)
    {
        // Engine.GetSingleton("Steam").Call("findLeaderboard", boardId);
    }

    public void SubmitScore(string boardId, long score)
    {
        // Engine.GetSingleton("Steam").Call("uploadLeaderboardScore", score, true);
    }
}
