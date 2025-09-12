using UnityEngine;

public abstract class PlayerMovementManager : MonoBehaviour
{

    public GameObject Mouse;
    public GameObject Mech;

    public PlayerMovementState MouseMovementState;
    public PlayerMovementState MechMovementState;

    // Variables
    public float MouseMoveSpeed = 8;
    public float MechMoveSpeed = 4;
    public float MechEnterDistance = 1.5f;


    // Keybinds
    public KeyCode SwitchKey = KeyCode.E;

    public abstract void SwitchState(PlayerMovementState state);
}
