using UnityEngine;

public class MovementConfig
{
    public GameObject Entity;
    public float MoveSpeed;
    public string HorizontalInput;
    public string VerticalInput;

    public MovementConfig(GameObject entity, float moveSpeed, string horizontalInput, string verticalInput)
    {
        Entity = entity;
        MoveSpeed = moveSpeed;
        HorizontalInput = horizontalInput;
        VerticalInput = verticalInput;
    }
}