using Godot;

namespace ArcoreGameCore;

[GlobalClass]
public partial class CharacterDefinition : Resource
{
    [Export] public string CharacterId { get; set; } = string.Empty;
    [Export] public string DisplayName { get; set; } = string.Empty;
    [Export] public Texture2D? Portrait { get; set; }
    [Export] public SpriteFrames? AnimationFrames { get; set; }
    [Export] public float MoveSpeed { get; set; } = 200f;
    [Export] public float JumpForce { get; set; } = 400f;
    [Export] public int MaxHealth { get; set; } = 3;
    [Export] public string[] UnlockAchievements { get; set; } = System.Array.Empty<string>();
    [Export] public AbilityDefinition[] Abilities { get; set; } = System.Array.Empty<AbilityDefinition>();
}
