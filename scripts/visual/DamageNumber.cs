using Godot;

namespace ArcoreGameCore;

/// <summary>
/// Floating damage/score number. Instantiate via DamageNumber.Spawn().
/// Place a PackedScene with this script in res://scenes/effects/DamageNumber.tscn.
/// </summary>
public partial class DamageNumber : Node2D
{
    private static PackedScene? _scene;

    [Export] public float FloatDistance { get; set; } = 60f;
    [Export] public float Duration { get; set; } = 0.8f;

    private Label _label = null!;

    public static void Spawn(Node parent, Vector2 position, string text, Color? color = null)
    {
        _scene ??= GD.Load<PackedScene>("res://scenes/effects/DamageNumber.tscn");
        var instance = _scene.Instantiate<DamageNumber>();
        parent.AddChild(instance);
        instance.GlobalPosition = position;
        instance._label.Text = text;
        instance._label.Modulate = color ?? Colors.White;
    }

    public override void _Ready()
    {
        _label = GetNode<Label>("Label");

        var tween = CreateTween();
        tween.SetParallel();
        tween.TweenProperty(this, "position:y", Position.Y - FloatDistance, Duration);
        tween.TweenProperty(_label, "modulate:a", 0f, Duration).SetDelay(Duration * 0.5f);
        tween.Chain().TweenCallback(Callable.From(QueueFree));
    }
}
