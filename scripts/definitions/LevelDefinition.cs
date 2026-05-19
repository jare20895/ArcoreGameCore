using Godot;

namespace ArcoreGameCore;

[GlobalClass]
public partial class LevelDefinition : Resource
{
    [Export] public string LevelId { get; set; } = string.Empty;
    [Export] public string DisplayName { get; set; } = string.Empty;
    [Export] public PackedScene? Scene { get; set; }
    [Export] public Texture2D? Thumbnail { get; set; }
    [Export] public AudioStream? Bgm { get; set; }
    [Export] public int ChapterIndex { get; set; }
    [Export] public int LevelIndex { get; set; }
    [Export] public string UnlockRequirement { get; set; } = string.Empty;
    [Export] public LevelDefinition? NextLevel { get; set; }
}
