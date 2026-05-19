using Godot;

namespace ArcoreGameCore;

[GlobalClass]
public partial class AbilityDefinition : Resource
{
    [Export] public string AbilityId { get; set; } = string.Empty;
    [Export] public string DisplayName { get; set; } = string.Empty;
    [Export] public Texture2D? Icon { get; set; }
    [Export] public float Cooldown { get; set; } = 0f;
    [Export] public int ManaCost { get; set; } = 0;
    [Export] public PackedScene? EffectScene { get; set; }
    [Export] public AudioStream? CastSound { get; set; }
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = string.Empty;
}
