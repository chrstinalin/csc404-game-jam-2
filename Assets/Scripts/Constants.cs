using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Config
{
    public static float MECH_MOVE_SPEED = 6;
    public static float MOUSE_MOVE_SPEED = 7;

    public static float MECH_JUMP_FORCE = 0f;
    public static float MOUSE_JUMP_FORCE = 5f;

    public static float MECH_DASH_SPEED = 30f;
    public static float MOUSE_DASH_SPEED = 40f;

    public static int MECH_INVENTORY_SIZE = 3;
    public static int MOUSE_INVENTORY_SIZE = 1;

    public static float MECH_ENTER_DISTANCE = 2.5f;
    public static float SMOOTH_TIME = 0.05f;

    public static float MIN_PITCH = 0f;
    public static float MAX_PITCH = 70f;

    public static float CAMERA_ZOOM_MULTIPLIER = 4f;
    public static float CAMERA_MIN_ZOOM = 10f;
    public static float CAMERA_MAX_ZOOM = 50f;
    public static float CAMERA_VELOCITY = 0f;
    public static float CAMERA_SMOOTH_TIME = 0.15f;
    public static float CAMERA_LOCK_ON_FOV = 70f;
    public static float CAMERA_DEFAULT_FOV = 50f;

    public static float CAMERA_COLLISION_RADIUS = 0.3f;
    public static float CAMERA_COLLISION_BUFFER = 0.05f;
    public static float CAMERA_MIN_DISTANCE = 0.2f;
    public static float CAMERA_COLLISION_SMOOTH_TIME = 0.05f;
    public static float CAMERA_COLLISION_EASE_TIME = 0.15f;

    public static float ZOOM_SENSITIVITY = 10f;

    public static float MOUSE_MAX_ZOOM = 16f;
    public static float MECH_MAX_ZOOM = 20f;

    public static float MOUSE_SENSITIVITY = 100f;

    public static float MIN_AI_DISTANCE = 2f;
    public static float STUCK_CHECK_INTERVAL = 0.5f;
    public static float STUCK_THRESHOLD = 0.2f;
    public static float ATTACK_RANGE = 15f;

    public static float PLAYER_MARKER_MOVE_SPEED = MOUSE_MOVE_SPEED;
    public static float PLAYER_MARKER_GROUND_RAY_HEIGHT = 50f;
    public static float PLAYER_MARKER_GROUND_SNAP_OFFSET = 0.05f;
    public static LayerMask PLAYER_MARKER_GROUND_LAYERS = ~0;

    public static float ENEMY_OUTLINE_WIDTH = 2f;
    public static float SELECTED_ENEMY_OUTLINE_WIDTH = 6f;

    public static Vector3 INTERACTABLE_TEXT_OFFSET = new Vector3(0, 0.2f, 0);

    public static float MOVE_EPSILON_SQR = 0.01f;

    public static float FOOTSTEP_INTERVAL = 0.3f;

    public static float LOCK_ON_AXIS_THRESHOLD = 0.5f;

    public static int SENSITIVITY_MULTIPLIER_DEFAULT = 10;
    public static int SENSITIVITY_MULTIPLIER_MAX = 20;

}

public enum AIState
{
    Idle,
    Walk,
    Attack,
    Chase
}

public static class Constant
{
    // Joystick Axes Constants
    public const string HORIZONTAL_JOY_RIGHT = "HorizontalRightJoystick";
    public const string HORIZONTAL_JOY_LEFT = "Horizontal";
    public const string VERTICAL_JOY_RIGHT = "VerticalRightJoystick";
    public const string VERTICAL_JOY_LEFT = "Vertical";

    public static readonly Joystick JOY_RIGHT = new Joystick(HORIZONTAL_JOY_RIGHT, VERTICAL_JOY_RIGHT);
    public static readonly Joystick JOY_LEFT = new Joystick(HORIZONTAL_JOY_LEFT, VERTICAL_JOY_LEFT);
}

public struct Joystick
{
    public string Horizontal { get; }
    public string Vertical { get; }

    public Joystick(string horizontal, string vertical)
    {
        Horizontal = horizontal;
        Vertical = vertical;
    }
}

public enum CardinalDirection
{
    North,
    South,
    East,
    West
}

public enum DeviceType
{
    Keyboard,
    Xbox
}

public enum PlatformMoveDirection { Vertical, Horizontal }

public static class GridDirection
{
    public static readonly Vector3Int North = new Vector3Int(0, 1, 0);
    public static readonly Vector3Int South = new Vector3Int(0, -1, 0);
    public static readonly Vector3Int East  = new Vector3Int(1, 0, 0);
    public static readonly Vector3Int West  = new Vector3Int(-1, 0, 0);
}

public enum ActionType
{
    Lockon,
    Shoot,
    SwitchCharacter,
    Sneak,
    Interact,
    Jump,
    Movement,
    Camera,
    Pause,
    RestartLevel
}

public static class ButtonMappings
{
    public static readonly Dictionary<ActionType, Dictionary<DeviceType, string>> Mappings = new()
    {
        { ActionType.Lockon, new() {
            { DeviceType.Keyboard, "Tab" },
            { DeviceType.Xbox, "LB/RB" },
        }},
        { ActionType.SwitchCharacter, new() {
            { DeviceType.Keyboard, "F" },
            { DeviceType.Xbox, "Y" },
        }},
        { ActionType.Interact, new() {
            { DeviceType.Keyboard, "E" },
            { DeviceType.Xbox, "B" },
        }},
        { ActionType.Jump, new() {
            { DeviceType.Keyboard, "Space" },
            { DeviceType.Xbox, "A" },
        }},
        { ActionType.Shoot, new() {
            { DeviceType.Keyboard, "E" },
            { DeviceType.Xbox, "LT/RT" },
        }}
    };

    public static string GetButtonLabel(ActionType action)
    {
        var names = Input.GetJoystickNames();
        var hasController = names != null && names.Any(n => !string.IsNullOrWhiteSpace(n));
        var device = hasController ? DeviceType.Xbox : DeviceType.Keyboard;
        if (Mappings.TryGetValue(action, out var deviceMappings) &&
            deviceMappings.TryGetValue(device, out var label))
        {
            return label;
        }
        return "NULL";
    }
}
