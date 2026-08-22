using UnityEngine;

public class ScrapCurrency : MonoBehaviour, ICarryable
{
    public int HPRestoreAmount;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialScale;

    [SerializeField] private float carryScale = 0.3f;
    [SerializeField] private InteractableObject interactableObject;

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

        if (interactableObject != null)
        {
            interactableObject.ChangeText("Drop Scrap / Heal D.K (when Nearby)");
            interactableObject.ForceRefresh();
        }
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

        if (interactableObject != null)
        {
            interactableObject.ChangeText("Pick Up Scrap");
        }
    }
}