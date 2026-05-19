using Godot;

namespace ArcoreGameCore;

[GlobalClass]
public partial class AchievementDefinition : Resource
{
    [Export] public string AchievementId { get; set; } = string.Empty;
    [Export] public string DisplayName { get; set; } = string.Empty;
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = string.Empty;
    [Export] public Texture2D? Icon { get; set; }
    [Export] public bool IsHidden { get; set; }
    [Export] public string StatTracked { get; set; } = string.Empty;
    [Export] public int StatThreshold { get; set; }
}
