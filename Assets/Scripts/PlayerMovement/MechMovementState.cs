using UnityEngine;
using UnityEngine.InputSystem;

public class MechMovementState : PlayerMovementState
{
    GameObject Mouse;
    GameObject Mech;
    private float _CurrentVelocity;
    private float _MechMaxZoom = 10f;

    public override void EnterState(PlayerMovementManager manager)
    {
        Mouse = manager.Mouse;
        Mech = manager.Mech;

        Mouse.SetActive(false);
        manager.CameraManager.SetFollowEntity(Mech);
        manager.CameraManager.SetMaxZoom(_MechMaxZoom);
        manager.CameraManager.PanTo(_MechMaxZoom);
    }

    public override void UpdateState(PlayerMovementManager manager)
    {
        Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

        if (moveDirection.sqrMagnitude == 0) return;

        // Movement
        Mech.transform.position += moveDirection * manager.MechMoveSpeed * Time.deltaTime;

        // Rotation
        var targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
        var angle = Mathf.SmoothDampAngle(Mech.transform.eulerAngles.y, targetAngle, ref _CurrentVelocity, manager.SmoothTime);
        Mech.transform.rotation = Quaternion.Euler(0.0f, targetAngle, 0.0f);

        // The player can leave the Mech at any time
        if (Input.GetKeyDown(manager.SwitchKey))
        {
            manager.SwitchState(manager.MouseMovementState);
            Mouse.transform.position = Mech.transform.position + Mech.transform.forward * -2;
        }

    }
}
