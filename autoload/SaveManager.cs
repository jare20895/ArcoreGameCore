using Godot;
using Godot.Collections;

namespace ArcoreGameCore;

/// <summary>
/// Handles save/load for game progress. Supports multiple slots and cloud delegation.
/// </summary>
public partial class SaveManager : Node
{
    public static SaveManager Instance { get; private set; } = null!;

    [Signal] public delegate void SaveCompletedEventHandler(int slot);
    [Signal] public delegate void LoadCompletedEventHandler(int slot);

    private const string SaveDir = "user://saves/";
    private const string SaveFileFormat = "user://saves/slot_{0}.json";

    public override void _Ready()
    {
        Instance = this;
        DirAccess.MakeDirRecursiveAbsolute(SaveDir);
    }

    public void Save(int slot, Dictionary data)
    {
        data["timestamp"] = Time.GetUnixTimeFromSystem();
        data["version"] = ProjectSettings.GetSetting("application/config/version", "1.0").AsString();

        var path = string.Format(SaveFileFormat, slot);
        using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Write);
        if (file == null)
        {
            GD.PushError($"SaveManager: failed to open {path} for writing");
            return;
        }

        file.StoreString(Json.Stringify(data));
        PlatformBridge.Instance?.SaveCloudFile(path, file.GetBuffer((long)file.GetLength()));
        EmitSignal(SignalName.SaveCompleted, slot);
    }

    public Dictionary? Load(int slot)
    {
        var path = string.Format(SaveFileFormat, slot);
        if (!Godot.FileAccess.FileExists(path))
            return null;

        using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
        if (file == null) return null;

        var json = new Json();
        if (json.Parse(file.GetAsText()) != Error.Ok)
        {
            GD.PushError($"SaveManager: failed to parse save slot {slot}");
            return null;
        }

        EmitSignal(SignalName.LoadCompleted, slot);
        return json.Data.AsGodotDictionary();
    }

    public bool SlotExists(int slot)
    {
        return Godot.FileAccess.FileExists(string.Format(SaveFileFormat, slot));
    }

    public void DeleteSlot(int slot)
    {
        var path = string.Format(SaveFileFormat, slot);
        if (Godot.FileAccess.FileExists(path))
            DirAccess.RemoveAbsolute(path);
    }
}
