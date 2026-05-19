using Godot;

namespace ArcoreGameCore;

/// <summary>
/// Wraps Godot's InputMap with named game actions and controller glyph awareness.
/// </summary>
public partial class InputManager : Node
{
    public static InputManager Instance { get; private set; } = null!;

    [Signal] public delegate void ControllerTypeChangedEventHandler(bool isGamepad);

    public bool IsGamepad { get; private set; }

    // Core action names — add your game-specific ones here
    public const string ActionMoveLeft = "move_left";
    public const string ActionMoveRight = "move_right";
    public const string ActionMoveUp = "move_up";
    public const string ActionMoveDown = "move_down";
    public const string ActionJump = "jump";
    public const string ActionAttack = "attack";
    public const string ActionInteract = "interact";
    public const string ActionPause = "pause";

    public override void _Ready()
    {
        Instance = this;
        EnsureDefaultActions();
    }

    public override void _Input(InputEvent @event)
    {
        bool wasGamepad = IsGamepad;
        if (@event is InputEventJoypadButton or InputEventJoypadMotion)
            IsGamepad = true;
        else if (@event is InputEventKey or InputEventMouseButton)
            IsGamepad = false;

        if (IsGamepad != wasGamepad)
            EmitSignal(SignalName.ControllerTypeChanged, IsGamepad);
    }

    public Vector2 GetMovementVector()
    {
        return Input.GetVector(ActionMoveLeft, ActionMoveRight, ActionMoveUp, ActionMoveDown);
    }

    public bool IsActionJustPressed(string action) => Input.IsActionJustPressed(action);
    public bool IsActionPressed(string action) => Input.IsActionPressed(action);
    public bool IsActionJustReleased(string action) => Input.IsActionJustReleased(action);

    private void EnsureDefaultActions()
    {
        // Only registers if not already defined in project InputMap
        var defaults = new System.Collections.Generic.Dictionary<string, Key>
        {
            { ActionMoveLeft,  Key.A },
            { ActionMoveRight, Key.D },
            { ActionMoveUp,    Key.W },
            { ActionMoveDown,  Key.S },
            { ActionJump,      Key.Space },
            { ActionAttack,    Key.Z },
            { ActionInteract,  Key.E },
            { ActionPause,     Key.Escape },
        };

        foreach (var (action, key) in defaults)
        {
            if (!InputMap.HasAction(action))
            {
                InputMap.AddAction(action);
                var ev = new InputEventKey { Keycode = key };
                InputMap.ActionAddEvent(action, ev);
            }
        }
    }
}
