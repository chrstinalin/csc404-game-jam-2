using UnityEngine;

public class Cheese : MonoBehaviour, ICarryable
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialScale;

    [SerializeField] private float carryScale = 0.5f;

    public Transform Transform => transform;

    void Awake()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialScale = transform.localScale;
    }

    public void ShrinkForCarry()
    {
        transform.localScale = initialScale * carryScale;
    }

    public void Drop(Vector3 dropPosition)
    {
        RaycastHit hit;
        if (Physics.Raycast(dropPosition + Vector3.up * 0.5f, Vector3.down, out hit, 100f))
        {
            transform.position = hit.point;
        }
        else
        {
            transform.position = dropPosition;
        }

        transform.rotation = initialRotation;
        transform.localScale = initialScale;
        gameObject.SetActive(true);
    }
}