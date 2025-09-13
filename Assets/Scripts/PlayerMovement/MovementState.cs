using UnityEngine;
public class MovementState : PlayerMovementState
{
    GameObject Entity;

    private float _CurrentVelocity;
    private float _MoveSpeed;

    private string _HorizontalInput;
    private string _VerticalInput;

    public override void EnterState(PlayerMovementManager manager, MovementConfig config)
    {
        Entity = config.Entity;
        _MoveSpeed = config.MoveSpeed;
        _VerticalInput = config.VerticalInput;
        _HorizontalInput = config.HorizontalInput;
    }

    public override void UpdateState(PlayerMovementManager manager)
    {
        Vector3 moveDirection = new Vector3(Input.GetAxis(_HorizontalInput), 0f, Input.GetAxis(_VerticalInput));

        if (moveDirection.sqrMagnitude == 0) return;

        // Movement
        Entity.transform.position += moveDirection * _MoveSpeed * Time.deltaTime;

        // Rotation
        var targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
        var angle = Mathf.SmoothDampAngle(Entity.transform.eulerAngles.y, targetAngle, ref _CurrentVelocity, manager.SmoothTime);
        Entity.transform.rotation = Quaternion.Euler(0.0f, targetAngle, 0.0f);
    }
}