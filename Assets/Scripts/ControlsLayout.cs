using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum ControllerType
{
    Keyboard,
    Xbox,
    PlayStation
}

public class ControlSchemeManager : MonoBehaviour
{
    [System.Serializable]
    public class ControlMapping
    {
        public ActionType actionType;
        public Text uiText; // The UI Text element to update
    }

    [Header("Control Mappings")]
    public ControlMapping[] controlMappings;

    [Header("Device Type Display")]
    public Text deviceTypeText; // Optional: displays "KEYBOARD", "XBOX CONTROLLER", etc.

    [Header("Update Settings")]
    public float checkInterval = 1f; // How often to check for controller changes
    
    private ControllerType currentControllerType;
    private float checkTimer;

    // Button mappings for each controller type
    private static readonly Dictionary<ActionType, Dictionary<ControllerType, string>> ButtonMappings = new()
    {
        { ActionType.Lockon, new() {
            { ControllerType.Keyboard, "TAB: LOCK ON" },
            { ControllerType.Xbox, "RT/RB: LOCK ON" },
            { ControllerType.PlayStation, "R2/R1: LOCK ON" },
        }},
        { ActionType.SwitchCharacter, new() {
            { ControllerType.Keyboard, "Q: SWITCH CHARACTER" },
            { ControllerType.Xbox, "Y: SWITCH CHARACTER" },
            { ControllerType.PlayStation, "△: SWITCH CHARACTER" },
        }},
        { ActionType.Sneak, new() {
            { ControllerType.Keyboard, "SHIFT: SNEAK" },
            { ControllerType.Xbox, "X: SNEAK" },
            { ControllerType.PlayStation, "□: SNEAK" },
        }},
        { ActionType.Interact, new() {
            { ControllerType.Keyboard, "E: INTERACT" },
            { ControllerType.Xbox, "B: INTERACT" },
            { ControllerType.PlayStation, "○: INTERACT" },
        }},
        { ActionType.Jump, new() {
            { ControllerType.Keyboard, "SPACE: JUMP" },
            { ControllerType.Xbox, "A: JUMP" },
            { ControllerType.PlayStation, "✕: JUMP" },
        }},
        { ActionType.Movement, new() {
            { ControllerType.Keyboard, "WASD: MOVEMENT" },
            { ControllerType.Xbox, "LEFT STICK: MOVEMENT" },
            { ControllerType.PlayStation, "LEFT STICK: MOVEMENT" },
        }},
        { ActionType.Camera, new() {
            { ControllerType.Keyboard, "MOUSE: CAMERA" },
            { ControllerType.Xbox, "RIGHT STICK: CAMERA" },
            { ControllerType.PlayStation, "RIGHT STICK: CAMERA" },
        }},
        { ActionType.Pause, new() {
            { ControllerType.Keyboard, "ESC: PAUSE" },
            { ControllerType.Xbox, "START: PAUSE" },
            { ControllerType.PlayStation, "OPTIONS: PAUSE" },
        }},
        { ActionType.RestartLevel, new() {
            { ControllerType.Keyboard, "R: RESTART LEVEL" },
            { ControllerType.Xbox, "BACK: RESTART LEVEL" },
            { ControllerType.PlayStation, "SHARE: RESTART LEVEL" },
        }}
    };

    private static readonly Dictionary<ControllerType, string> DeviceTypeNames = new()
    {
        { ControllerType.Keyboard, "KEYBOARD & MOUSE" },
        { ControllerType.Xbox, "XBOX CONTROLLER" },
        { ControllerType.PlayStation, "PLAYSTATION CONTROLLER" }
    };

    void Start()
    {
        currentControllerType = DetectControllerType();
        UpdateAllControls();
        UpdateDeviceType();
    }

    void Update()
    {
        // Periodically check if controller type has changed
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            ControllerType detectedType = DetectControllerType();
            
            if (detectedType != currentControllerType)
            {
                currentControllerType = detectedType;
                UpdateAllControls();
                UpdateDeviceType();
            }
        }
    }

    private ControllerType DetectControllerType()
    {
        var joystickNames = Input.GetJoystickNames();
        
        // Check if any controller is connected
        if (joystickNames == null || !joystickNames.Any(n => !string.IsNullOrWhiteSpace(n)))
        {
            return ControllerType.Keyboard;
        }

        // Get the first connected controller name
        string controllerName = joystickNames.FirstOrDefault(n => !string.IsNullOrWhiteSpace(n));
        
        if (string.IsNullOrEmpty(controllerName))
        {
            return ControllerType.Keyboard;
        }

        // Convert to lowercase for easier matching
        controllerName = controllerName.ToLower();

        // Detect controller type based on name
        if (controllerName.Contains("playstation") || 
            controllerName.Contains("ps4") || 
            controllerName.Contains("ps5") ||
            controllerName.Contains("dualshock") ||
            controllerName.Contains("dualsense"))
        {
            return ControllerType.PlayStation;
        }
        else if (controllerName.Contains("xbox") || 
                 controllerName.Contains("xinput"))
        {
            return ControllerType.Xbox;
        }

        // Default to Xbox for unknown controllers
        return ControllerType.Xbox;
    }

    private void UpdateAllControls()
    {
        foreach (var mapping in controlMappings)
        {
            if (mapping.uiText != null)
            {
                string buttonLabel = GetButtonLabel(mapping.actionType, currentControllerType);
                mapping.uiText.text = buttonLabel;
            }
        }
    }

    private void UpdateDeviceType()
    {
        if (deviceTypeText != null)
        {
            deviceTypeText.text = DeviceTypeNames[currentControllerType];
        }
    }

    public static string GetButtonLabel(ActionType action, ControllerType controller)
    {
        if (ButtonMappings.TryGetValue(action, out var controllerMappings) &&
            controllerMappings.TryGetValue(controller, out var label))
        {
            return label;
        }
        return "NULL";
    }

    // Public method to manually force a control scheme update
    public void ForceUpdate()
    {
        currentControllerType = DetectControllerType();
        UpdateAllControls();
        UpdateDeviceType();
    }
}