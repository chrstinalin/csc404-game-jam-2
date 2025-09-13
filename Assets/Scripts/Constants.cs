using System;

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