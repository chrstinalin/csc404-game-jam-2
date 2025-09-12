using UnityEngine;
using UnityEngine.InputSystem;

public class MouseMovementState : PlayerMovementState
{
    GameObject Mouse;
    GameObject Mech;
    private float _CurrentVelocity;
    private float _MouseMaxZoom = 4f;

    public override void EnterState(PlayerMovementManager manager)
    {
        Mouse = manager.Mouse;
        Mech = manager.Mech;

        Mouse.SetActive(true);
        manager.CameraManager.SetFollowEntity(Mouse);
        manager.CameraManager.SetMaxZoom(_MouseMaxZoom);
        manager.CameraManager.PanTo(_MouseMaxZoom);
    }

    public override void UpdateState(PlayerMovementManager manager)
    {
        Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
    
        if (moveDirection.sqrMagnitude == 0) return;

        // Movement
        Mouse.transform.position += moveDirection * manager.MouseMoveSpeed * Time.deltaTime;

        // Rotation
        var targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
        var angle = Mathf.SmoothDampAngle(Mouse.transform.eulerAngles.y, targetAngle, ref _CurrentVelocity, manager.SmoothTime);
        Mouse.transform.rotation = Quaternion.Euler(0.0f, targetAngle, 0.0f);

        // the Mouse can only enter the Mech when within proximity
        float distance = Vector3.Distance(Mouse.transform.position, Mech.transform.position);
        if (distance < manager.MechEnterDistance && Input.GetKeyDown(manager.SwitchKey))
        {
            manager.SwitchState(manager.MechMovementState);
        }
    }
}