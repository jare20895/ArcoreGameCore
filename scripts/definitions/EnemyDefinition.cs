using Godot;

namespace ArcoreGameCore;

[GlobalClass]
public partial class EnemyDefinition : Resource
{
    [Export] public string EnemyId { get; set; } = string.Empty;
    [Export] public string DisplayName { get; set; } = string.Empty;
    [Export] public SpriteFrames? AnimationFrames { get; set; }
    [Export] public int MaxHealth { get; set; } = 1;
    [Export] public int ContactDamage { get; set; } = 1;
    [Export] public float MoveSpeed { get; set; } = 80f;
    [Export] public float DetectionRange { get; set; } = 200f;
    [Export] public int ScoreValue { get; set; } = 100;
    [Export] public bool CanBeStomped { get; set; } = true;
    [Export] public PackedScene? DeathEffect { get; set; }
    [Export] public AudioStream? DeathSound { get; set; }
}
