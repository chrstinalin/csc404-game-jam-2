using UnityEngine;

public class Cheese : MonoBehaviour, ICarryable
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialScale;
    private Outline outline;

    [SerializeField] private float carryScale = 0.5f;
    [SerializeField] private InteractableObject interactableObject;


    [Header("Pickup Settings")]
    public bool startPickedUp = false;

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
        interactableObject.ChangeText("");
    }

    public void Drop(Vector3 dropPosition)
    {
        // You can no longer drop the cheese :)
    }

    private void OnDisable()
    {
        RemoveFromInventoryIfCarried();
    }

    private void OnDestroy()
    {
        RemoveFromInventoryIfCarried();
    }

    private void RemoveFromInventoryIfCarried()
    {
        if (PlayerMouse.Instance.InventoryManager != null && PlayerMouse.Instance.InventoryManager.GetCarriedItem() == this)
        {
            PlayerMouse.Instance.InventoryManager.RemoveCarriedItem();
        }
    }
}