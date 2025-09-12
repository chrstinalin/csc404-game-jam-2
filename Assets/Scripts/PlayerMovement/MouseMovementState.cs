using UnityEngine;
using UnityEngine.InputSystem;

public class MouseMovementState : PlayerMovementState
{
    GameObject Mouse;
    GameObject Mech;

    public override void EnterState(PlayerMovementManager manager)
    {
        Mouse = manager.Mouse;
        Mech = manager.Mech;
    }

    public override void UpdateState(PlayerMovementManager manager)
    {
        Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        Mouse.transform.position += moveDirection * manager.MouseMoveSpeed * Time.deltaTime;

        // the Mouse can only enter the Mech when within proximity
        float distance = Vector3.Distance(Mouse.transform.position, Mech.transform.position);
        if (distance < manager.MechEnterDistance && Input.GetKeyDown(manager.SwitchKey))
        {
            Mouse.SetActive(false);
            manager.SwitchState(manager.MechMovementState);
        }
    }
}