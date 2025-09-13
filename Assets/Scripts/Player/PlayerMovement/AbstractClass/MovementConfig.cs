using UnityEngine;

public class MovementConfig
{
    public GameObject Entity;
    public float MoveSpeed;

    public MovementConfig(GameObject entity, float moveSpeed)
    {
        Entity = entity;
        MoveSpeed = moveSpeed;
    }
}