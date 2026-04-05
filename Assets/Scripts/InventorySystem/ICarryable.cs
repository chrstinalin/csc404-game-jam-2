using UnityEngine;

public interface ICarryable
{
    void Drop(Vector3 dropPosition);
    Transform Transform { get; }
}