using UnityEngine;
using static UnityEngine.Rendering.STP;
public class MovementState : PlayerMovementState
{
    GameObject Entity;

    private float _CurrentVelocity;
    private float _MoveSpeed;
    private Joystick _Input = Constant.JOY_LEFT;

    public override void EnterState(PlayerMovementManager manager, MovementConfig config)
    {
        Entity = config.Entity;
        _MoveSpeed = config.MoveSpeed;
    }

    public override void UpdateState(PlayerMovementManager manager)
    {
        Vector3 moveDirection = new Vector3(Input.GetAxis(_Input.Horizontal), 0f, Input.GetAxis(_Input.Vertical));

        if (moveDirection.sqrMagnitude == 0) return;

        // Movement
        Entity.transform.position += moveDirection * _MoveSpeed * Time.deltaTime;

        // Rotation
        var targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
        var angle = Mathf.SmoothDampAngle(Entity.transform.eulerAngles.y, targetAngle, ref _CurrentVelocity, manager.SmoothTime);
        Entity.transform.rotation = Quaternion.Euler(0.0f, targetAngle, 0.0f);
    }
    public override void UpdateJoyStick(Joystick Input) => _Input = Input;

}