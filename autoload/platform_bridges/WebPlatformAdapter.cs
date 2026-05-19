using Godot;

namespace ArcoreGameCore;

/// <summary>
/// Web export adapter. Uses JavaScriptBridge for browser-side integrations (itch.io, Newgrounds, etc.).
/// </summary>
public class WebPlatformAdapter : IPlatformAdapter
{
    public void Initialize()
    {
        GD.Print("[PlatformBridge] Web adapter initialised.");
    }

    public void UnlockAchievement(string id)
    {
        // Example: Newgrounds API via JS
        // JavaScriptBridge.Eval($"if(window.ng) ng.unlockMedal('{id}');");
        GD.Print($"[Web] UnlockAchievement: {id}");
    }

    public bool IsAchievementUnlocked(string id)
    {
        // Query localStorage or JS API
        var result = JavaScriptBridge.Eval($"localStorage.getItem('ach_{id}')");
        return result?.ToString() == "1";
    }

    public void SetStat(string id, int value)
    {
        JavaScriptBridge.Eval($"localStorage.setItem('stat_{id}', '{value}');");
    }

    public void SaveCloudFile(string path, byte[] data)
    {
        // Web: save to localStorage or post to server
        var b64 = Marshalls.RawToBase64(data);
        JavaScriptBridge.Eval($"localStorage.setItem('save_{path}', '{b64}');");
    }

    public void SetRichPresence(string status) { /* not applicable on web */ }

    public void ShowLeaderboard(string boardId)
    {
        // Open leaderboard overlay via JS callback
        JavaScriptBridge.Eval($"if(window.showLeaderboard) showLeaderboard('{boardId}');");
    }

    public void SubmitScore(string boardId, long score)
    {
        JavaScriptBridge.Eval($"if(window.submitScore) submitScore('{boardId}', {score});");
    }
}
