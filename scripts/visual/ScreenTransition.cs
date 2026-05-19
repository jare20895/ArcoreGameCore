using Godot;
using System;

namespace ArcoreGameCore;

/// <summary>
/// Full-screen fade/wipe overlay. Referenced by SceneRouter.
/// Add as a CanvasLayer child of any persistent UI node.
/// </summary>
public partial class ScreenTransition : Node
{
    public static ScreenTransition? Instance { get; private set; }

    [Export] public float Duration { get; set; } = 0.3f;
    [Export] public Color FadeColor { get; set; } = Colors.Black;

    private ColorRect _overlay = null!;
    private Tween? _tween;

    public override void _Ready()
    {
        Instance = this;

        _overlay = new ColorRect
        {
            Color = new Color(FadeColor, 0f),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            LayoutMode = 1,
            AnchorsPreset = (int)Control.LayoutPreset.FullRect
        };

        var layer = new CanvasLayer { Layer = 100 };
        AddChild(layer);
        layer.AddChild(_overlay);
    }

    public void PlayOutro(Action onComplete)
    {
        _tween?.Kill();
        _tween = CreateTween();
        _tween.TweenProperty(_overlay, "color:a", 1f, Duration);
        _tween.TweenCallback(Callable.From(() =>
        {
            onComplete();
            PlayIntro();
        }));
    }

    public void PlayIntro()
    {
        _tween?.Kill();
        _tween = CreateTween();
        _tween.TweenProperty(_overlay, "color:a", 0f, Duration);
    }
}
