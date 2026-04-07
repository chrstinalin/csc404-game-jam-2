using UnityEngine;

public class Cheese : MonoBehaviour, ICarryable
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialScale;

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
        interactableObject.ChangeText("Drop C.H.E.E.S.E");
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
        interactableObject.ChangeText("Pick Up C.H.E.E.S.E");
    }

    public void ResetToInitial()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        transform.localScale = initialScale;
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