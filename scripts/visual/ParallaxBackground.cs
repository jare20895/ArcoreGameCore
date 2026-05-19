using Godot;

namespace ArcoreGameCore;

/// <summary>
/// Infinite auto-scrolling parallax layer. Add multiple instances with different speeds.
/// Replaces the Phaser TileSprite parallax pattern.
/// </summary>
public partial class ParallaxBackground : Node2D
{
    [Export] public Texture2D? Texture { get; set; }
    [Export] public float ScrollSpeedX { get; set; } = 30f;
    [Export] public float ScrollSpeedY { get; set; } = 0f;
    [Export] public float CameraInfluence { get; set; } = 0.5f;

    private Sprite2D[] _tiles = System.Array.Empty<Sprite2D>();
    private Vector2 _offset;
    private Vector2 _tileSize;

    public override void _Ready()
    {
        if (Texture == null) return;

        _tileSize = Texture.GetSize();
        var viewport = GetViewportRect().Size;

        int countX = Mathf.CeilToInt(viewport.X / _tileSize.X) + 2;
        _tiles = new Sprite2D[countX];

        for (int i = 0; i < countX; i++)
        {
            var sprite = new Sprite2D { Texture = Texture };
            AddChild(sprite);
            sprite.Position = new Vector2(i * _tileSize.X, 0);
            _tiles[i] = sprite;
        }
    }

    public override void _Process(double delta)
    {
        _offset.X += ScrollSpeedX * (float)delta;
        _offset.Y += ScrollSpeedY * (float)delta;

        if (_offset.X > _tileSize.X) _offset.X -= _tileSize.X;
        if (_offset.X < 0) _offset.X += _tileSize.X;

        Position = new Vector2(-_offset.X, -_offset.Y);
    }
}
