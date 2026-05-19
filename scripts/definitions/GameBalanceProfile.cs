using Godot;

namespace ArcoreGameCore;

/// <summary>
/// Swap entire balance profiles at runtime (e.g. Easy / Normal / Hard) without touching code.
/// </summary>
[GlobalClass]
public partial class GameBalanceProfile : Resource
{
    [Export] public string ProfileName { get; set; } = "Normal";
    [Export] public float PlayerSpeedMultiplier { get; set; } = 1f;
    [Export] public float EnemySpeedMultiplier { get; set; } = 1f;
    [Export] public float DamageMultiplier { get; set; } = 1f;
    [Export] public float ScoreMultiplier { get; set; } = 1f;
    [Export] public int StartingLives { get; set; } = 3;
    [Export] public bool InfiniteLives { get; set; }
    [Export] public bool EnemiesRespawn { get; set; } = true;
}
