using UnityEngine;

/**
 * This script manages the player's movement state, and
 * switching between moving the mouse and the mech.
 **/
public class MovementManager : PlayerMovementManager
{

    PlayerMovementState currState;

    void Start()
    {
        MouseMovementState = new MouseMovementState();
        MechMovementState = new MechMovementState();
        currState = MechMovementState;
        currState.EnterState(this);
        Mouse.SetActive(false);
    }

    void Update()
    {
        currState.UpdateState(this);
        CameraManager.UpdateCamera();
    }

    public override void SwitchState(PlayerMovementState state)
    {
        currState = state;
        currState.EnterState(this);
    }
}
