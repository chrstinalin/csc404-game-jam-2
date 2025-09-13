using UnityEngine;

public abstract class PlayerMovementManager : MonoBehaviour
{

    public CameraMovementManager CameraManager;
    public GameObject Mouse;
    public GameObject Mech;

    public PlayerMovementState MouseMovementState;
    public PlayerMovementState MechMovementState;

    public bool IsMouseActive;

    // Default Variables
    public float MouseMoveSpeed = 8;
    public float MechMoveSpeed = 4;
    public float MechEnterDistance = 2.5f;
    public float SmoothTime = 0.05f;

    // Keybinds
    public KeyCode MountKey = KeyCode.Joystick1Button0;

    public abstract void ToggleMouse(bool toggle);
}
