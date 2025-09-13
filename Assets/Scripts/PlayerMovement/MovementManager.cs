using UnityEngine;

/**
 * This script manages the player's movement state, and
 * switching between moving the mouse and the mech.
 **/
public class MovementManager : PlayerMovementManager
{
    private float _MouseMaxZoom = 4f;
    private float _MechMaxZoom = 10f;

    void Start()
    {
        MovementConfig MouseConfig = new(Mouse, MouseMoveSpeed, Constant.HORIZONTAL_JOY_RIGHT, Constant.VERTICAL_JOY_RIGHT);
        MovementConfig MechConfig = new(Mech, MouseMoveSpeed, Constant.HORIZONTAL_JOY_LEFT, Constant.VERTICAL_JOY_LEFT);

        MouseMovementState = new MovementState();
        MechMovementState = new MovementState();

        MouseMovementState.EnterState(this, MouseConfig);
        MechMovementState.EnterState(this, MechConfig);

        ToggleMouse(false);
    }

    void Update()
    {
        if (IsMouseActive) MouseMovementState.UpdateState(this);
        MechMovementState.UpdateState(this);
        CameraManager.UpdateCamera();

        if (Input.GetKeyDown(MountKey))
        {
            if (!IsMouseActive)  ToggleMouse(true);
            else if (Vector3.Distance(Mouse.transform.position, Mech.transform.position) < MechEnterDistance) ToggleMouse(false);
        }
    }

    public override void ToggleMouse(bool toggle)
    {
        IsMouseActive = toggle;
        Mouse.SetActive(toggle);

        if (!IsMouseActive) CameraManager.SetFollowEntity(Mech, _MechMaxZoom);
        else
        {
            CameraManager.SetFollowEntity(Mouse, _MouseMaxZoom);
            Mouse.transform.position = Mech.transform.position + Mech.transform.forward * -2;
        }        
    }
}
