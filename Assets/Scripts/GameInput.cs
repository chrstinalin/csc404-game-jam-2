using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;

/*
 * Single entry point for every control the game reads.
 *
 * Gamepads go through the Input System package, never the legacy Input Manager.
 * The legacy API addresses a pad by raw axis/button *index*, and those indices
 * differ per pad and per platform - a DualShock 4 reports the right stick on
 * axes 3/4 where an Xbox pad reports 4/5, and its face buttons sit one slot
 * over. That is why InputManager.asset (authored against an Xbox pad) sent the
 * DS4's right-stick Y into the camera's horizontal axis, left right-stick X
 * unbound, and put Pause on R2 instead of Options. The Input System normalises
 * every pad onto one layout (buttonSouth, rightStick, startButton, ...), so the
 * mapping below is correct for DS4/DualSense/Xbox alike.
 *
 * Keyboard & mouse stay on the legacy API - that path already worked, and the
 * scroll/mouse-delta scaling in InputManager.asset is what the camera is tuned
 * against.
 */
public static class GameInput
{
    // A DS4/DualSense trigger rests at 0 and travels to 1 under the Input
    // System, so a single press point works for both pad families.
    private const float TRIGGER_PRESS_POINT = 0.2f;

    // Matches the 0.19 dead zone the Xbox pad was tuned with in
    // InputManager.asset - the Input System's own default is a looser 0.125,
    // which would let a worn stick drift the camera.
    private const float STICK_DEAD_ZONE = 0.19f;

    // How far a stick has to travel along one axis before it counts as a menu
    // step. Menus read both axes at once (a list scrolls vertically while a
    // slider drags horizontally), so a step also has to be the dominant axis.
    private const float MENU_STEP_THRESHOLD = 0.5f;

    private static Gamepad Pad => Gamepad.current;

    /*
     * ========================================================================
     * Sticks
     * ========================================================================
     */

    /// Left stick + WASD/arrows. Movement.
    public static Vector2 Move => ReadStick(StickSide.Left);

    /// How far the mouse moved since last frame, in legacy "Mouse X/Y" units
    /// (raw pixels times the 0.1 scaling in InputManager.asset).
    ///
    /// This is a displacement, not a rate: the frames it arrives over already
    /// add up to the distance the hand moved. Scaling it by Time.deltaTime -
    /// as the camera used to - both shrinks it and makes the same hand motion
    /// turn less the higher the framerate runs. Callers must not.
    public static Vector2 LookDelta => new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

    /// Right stick + arrow keys, -1..1. This one *is* a rate held over time,
    /// so callers scale it by Time.deltaTime.
    public static Vector2 LookStick
    {
        get
        {
            Vector2 look = KeyboardLook();

            if (Pad != null)
                look += DeadZone(Pad.rightStick.ReadValue());

            return Vector2.ClampMagnitude(look, 1f);
        }
    }

    /// Reads one stick, merged with its keyboard equivalent. The mech follows
    /// whichever stick MovementManager hands it, so both sides are addressable.
    public static Vector2 ReadStick(StickSide side)
    {
        Vector2 value = side == StickSide.Left ? KeyboardMove() : KeyboardLook();

        if (Pad != null)
        {
            // The d-pad only doubles for the left stick; on the right it would
            // fight with lock-on target cycling.
            value += side == StickSide.Left
                ? DeadZone(Pad.leftStick.ReadValue()) + Pad.dpad.ReadValue()
                : DeadZone(Pad.rightStick.ReadValue());
        }

        return Vector2.ClampMagnitude(value, 1f);
    }

    /// Right-stick horizontal only - used to flick between lock-on targets.
    public static float TargetCycleAxis => Pad != null ? DeadZone(Pad.rightStick.ReadValue()).x : 0f;

    /*
     * ========================================================================
     * Gameplay buttons
     * ========================================================================
     */

    // Everything that acts on the world stops at the pause menu. Interact sits
    // on the left mouse button, and PushableObject/Lever/the inventories each
    // run their own Update() with no pause check of their own - so without this
    // a click on a pause-menu button would also grab the crate behind it.
    // Releases are never blocked: see InteractUp.
    private static bool GameplayLocked => UIManager.GamePaused;

    /// Cross / A, or Space.
    public static bool JumpDown => !GameplayLocked && (Input.GetKeyDown(KeyCode.Space) || PadDown(Pad?.buttonSouth));

    /// Circle / B, or left click / E.
    ///
    /// Left click is the primary: the right hand is already resting on the
    /// mouse and has nothing else bound to it in gameplay, so holding it to
    /// push a crate leaves the left hand free for WASD. E stays as an alias.
    public static bool InteractDown =>
        !GameplayLocked && (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0) || PadDown(Pad?.buttonEast));

    public static bool Interact =>
        !GameplayLocked && (Input.GetKey(KeyCode.E) || Input.GetMouseButton(0) || PadHeld(Pad?.buttonEast));

    // Not pause-gated. A release can only ever end an action, and swallowing it
    // would strand a crate push mid-hold: the push would freeze on pause, the
    // button come up unnoticed, and the mech stay locked to the box on resume.
    public static bool InteractUp =>
        Input.GetKeyUp(KeyCode.E) || Input.GetMouseButtonUp(0) || PadUp(Pad?.buttonEast);

    /// Square / X, or Left Shift.
    public static bool Sneak => !GameplayLocked && (Input.GetKey(KeyCode.LeftShift) || PadHeld(Pad?.buttonWest));

    /// Triangle / Y, or F. Swaps between mouse and mech.
    public static bool SwitchCharacterDown =>
        !GameplayLocked && (Input.GetKeyDown(KeyCode.F) || PadDown(Pad?.buttonNorth));

    /// L1/R1 (LB/RB), or Tab.
    public static bool SummonMecha => !GameplayLocked && (Input.GetKey(KeyCode.Tab) || Shoulders());

    /// L1/R1 (LB/RB), or Tab. Held to stay in lock-on.
    public static bool LockOnMode => !GameplayLocked && (Input.GetKey(KeyCode.Tab) || Shoulders());
    public static bool LockOnModeDown =>
        !GameplayLocked &&
        (Input.GetKeyDown(KeyCode.Tab) || PadDown(Pad?.leftShoulder) || PadDown(Pad?.rightShoulder));

    /// L2/R2 (LT/RT) as a 0..1 axis, or left click / E. Fires while locked on.
    public static float FireAxis
    {
        get
        {
            if (GameplayLocked) return 0f;
            if (Input.GetKey(KeyCode.E) || Input.GetMouseButton(0)) return 1f;
            if (Pad == null) return 0f;

            float triggers = Mathf.Max(Pad.leftTrigger.ReadValue(), Pad.rightTrigger.ReadValue());
            return triggers < TRIGGER_PRESS_POINT ? 0f : triggers;
        }
    }

    /// R3 (right stick click), or Q. Held to zoom with the right stick.
    public static bool ZoomModifier => Input.GetKey(KeyCode.Q) || PadHeld(Pad?.rightStickButton);

    /// Right stick vertical while ZoomModifier is held, or the scroll wheel.
    public static float ZoomAxis => Pad != null ? DeadZone(Pad.rightStick.ReadValue()).y : 0f;
    public static float ScrollAxis => Input.GetAxis("Mouse ScrollWheel");

    /// D/A. On a pad this is the right stick instead (see TargetCycleAxis) -
    /// binding the d-pad here too would cycle targets every time it was used to
    /// walk, since the d-pad doubles for the left stick.
    public static bool NextTargetDown => !GameplayLocked && Input.GetKeyDown(KeyCode.D);
    public static bool PreviousTargetDown => !GameplayLocked && Input.GetKeyDown(KeyCode.A);

    /*
     * ========================================================================
     * Menu buttons
     * ========================================================================
     */

    /// Options / Start, or P.
    public static bool PauseDown => Input.GetKeyDown(KeyCode.P) || PadDown(Pad?.startButton);

    /// Cross / A, or Enter/Space. Deliberately a different button from Cancel:
    /// the legacy asset had both on "joystick button 1", so one press on a DS4
    /// confirmed and backed out at the same time.
    public static bool SubmitDown =>
        Input.GetKeyDown(KeyCode.Return) ||
        Input.GetKeyDown(KeyCode.KeypadEnter) ||
        Input.GetKeyDown(KeyCode.Space) ||
        PadDown(Pad?.buttonSouth);

    /// Circle / B, or Escape.
    public static bool CancelDown => Input.GetKeyDown(KeyCode.Escape) || PadDown(Pad?.buttonEast);

    /// Raw -1/0/1 for menu navigation. Left stick, d-pad, WASD and arrows.
    public static float MenuVertical => MenuStep(true, KeyCode.W, KeyCode.UpArrow, KeyCode.S, KeyCode.DownArrow);
    public static float MenuHorizontal => MenuStep(false, KeyCode.D, KeyCode.RightArrow, KeyCode.A, KeyCode.LeftArrow);

    /*
     * ========================================================================
     * Pointer (menus only)
     * ========================================================================
     */

    public static bool PointerPressed => Input.GetMouseButton(0);
    public static Vector2 PointerPosition => Input.mousePosition;

    /// The EventSystem's InputSystemUIInputModule also raises Submit/Cancel on
    /// whatever is selected, and a Button turns that into an onClick. The menu
    /// scripts drive Submit/Cancel themselves - they own the SFX, the toggles
    /// and the panel stack - so with both live a single ✕/A press fires the
    /// selected button twice. Menus call this once they're up to stay the only
    /// source; pointer clicks are untouched.
    public static void TakeOverMenuSubmit()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null) return;

        InputSystemUIInputModule module = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (module == null) return;

        module.submit = null;
        module.cancel = null;
    }

    /*
     * ========================================================================
     * Device detection
     * ========================================================================
     */

    /// What the player is holding, for the on-screen control prompts. Uses the
    /// Input System's device layouts rather than the product name string, which
    /// a DS4 reports as the useless "Wireless Controller" on most platforms.
    public static ControllerType CurrentController
    {
        get
        {
            Gamepad pad = Pad;
            if (pad == null) return ControllerType.Keyboard;

            if (pad is DualShockGamepad) return ControllerType.PlayStation;
            if (pad is XInputController) return ControllerType.Xbox;

            // Fall back to the layout name for pads that aren't backed by a
            // platform-specific class (e.g. a DS4 seen over Bluetooth HID).
            string layout = pad.layout ?? string.Empty;
            if (layout.Contains("DualShock") || layout.Contains("DualSense") || layout.Contains("PS4") || layout.Contains("PS5"))
                return ControllerType.PlayStation;

            return ControllerType.Xbox;
        }
    }

    /*
     * ========================================================================
     * Helpers
     * ========================================================================
     */

    private static bool Shoulders() =>
        PadHeld(Pad?.leftShoulder) || PadHeld(Pad?.rightShoulder);

    private static bool PadDown(ButtonControl button) => button != null && button.wasPressedThisFrame;
    private static bool PadHeld(ButtonControl button) => button != null && button.isPressed;
    private static bool PadUp(ButtonControl button) => button != null && button.wasReleasedThisFrame;

    private static Vector2 KeyboardMove() => new Vector2(
        Axis(KeyCode.D, KeyCode.A),
        Axis(KeyCode.W, KeyCode.S)
    );

    private static Vector2 KeyboardLook() => new Vector2(
        Axis(KeyCode.RightArrow, KeyCode.LeftArrow),
        Axis(KeyCode.UpArrow, KeyCode.DownArrow)
    );

    private static Vector2 MenuStick()
    {
        if (Pad == null) return Vector2.zero;
        return DeadZone(Pad.leftStick.ReadValue()) + Pad.dpad.ReadValue();
    }

    /// Radial dead zone, rescaled so the live range still reaches a full 1.
    private static Vector2 DeadZone(Vector2 stick)
    {
        float magnitude = stick.magnitude;
        if (magnitude < STICK_DEAD_ZONE) return Vector2.zero;

        float scaled = Mathf.InverseLerp(STICK_DEAD_ZONE, 1f, magnitude);
        return stick / magnitude * Mathf.Min(scaled, 1f);
    }

    private static float Axis(KeyCode positive, KeyCode negative)
    {
        float value = 0f;
        if (Input.GetKey(positive)) value += 1f;
        if (Input.GetKey(negative)) value -= 1f;
        return value;
    }

    private static float MenuStep(bool vertical, KeyCode positive, KeyCode altPositive, KeyCode negative, KeyCode altNegative)
    {
        if (Input.GetKey(positive) || Input.GetKey(altPositive)) return 1f;
        if (Input.GetKey(negative) || Input.GetKey(altNegative)) return -1f;

        Vector2 stick = MenuStick();
        float axis = vertical ? stick.y : stick.x;
        float other = vertical ? stick.x : stick.y;

        if (Mathf.Abs(axis) < MENU_STEP_THRESHOLD || Mathf.Abs(axis) < Mathf.Abs(other))
            return 0f;

        return Mathf.Sign(axis);
    }
}
