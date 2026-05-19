using Godot;

namespace ArcoreGameCore;

/// <summary>
/// Add to a Camera2D node. Call Shake() from anywhere via CameraShake.Instance.
/// </summary>
public partial class CameraShake : Node
{
    public static CameraShake? Instance { get; private set; }

    [Export] public float DecayRate { get; set; } = 8f;

    private Camera2D _camera = null!;
    private float _trauma;
    private float _time;

    public override void _Ready()
    {
        Instance = this;
        _camera = GetParent<Camera2D>();
    }

    public override void _Process(double delta)
    {
        if (_trauma <= 0f) return;

        _trauma = Mathf.Max(0f, _trauma - DecayRate * (float)delta);
        float shake = _trauma * _trauma;
        _time += (float)delta * 60f;

        _camera.Offset = new Vector2(
            GD.Randf() * 2f - 1f,
            GD.Randf() * 2f - 1f
        ) * shake * 30f;
    }

    /// <param name="amount">0–1 trauma added. Stacks up to 1.</param>
    public void Shake(float amount) => _trauma = Mathf.Min(1f, _trauma + amount);

    public static void TriggerShake(float amount) => Instance?.Shake(amount);
}
