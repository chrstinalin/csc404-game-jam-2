using UnityEngine;
using UnityEngine.InputSystem;

public class MechMovementState : PlayerMovementState
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
        Mech.transform.position += moveDirection * manager.MechMoveSpeed * Time.deltaTime;

        // The player can leave the Mech at any time
        if (Input.GetKeyDown(manager.SwitchKey))
        {
            manager.SwitchState(manager.MouseMovementState);
            Mouse.transform.position = Mech.transform.position + new Vector3(1.5f, 0f, 0f);
            Mouse.SetActive(true);
        }

    }
}
