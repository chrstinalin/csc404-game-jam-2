using UnityEngine;

public class MouseInventoryManager : MonoBehaviour
{
    private ICarryable nearbyItem; 
    [HideInInspector] public ICarryable carriedItem; 
    private Transform carryPoint;
    private Animator animator;

    private void Awake()
    {
        carryPoint = transform.Find("CarryPoint");
        if (carryPoint == null)
            Debug.LogError("A Transform named CarryPoint must be a child of PlayerMouse");

        animator = GetComponentInChildren<Animator>();
        if (animator == null)
            Debug.LogError("No Animator found in children of PlayerMouse");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (MovementManager.Instance == null || !MovementManager.Instance.IsMouseActive)
            return;

        var carryable = other.GetComponent<ICarryable>();
        if (carryable != null)
        {
            nearbyItem = carryable;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var carryable = other.GetComponent<ICarryable>();
        if (carryable != null && carryable == nearbyItem)
        {
            nearbyItem = null;
        }
    }

    private void Update()
    {
        bool playerControllingMouse = MovementManager.Instance != null && MovementManager.Instance.IsMouseActive;
        if (!playerControllingMouse) return;

        if (Input.GetButtonDown("Interact"))
        {
            if (carriedItem == null && nearbyItem != null)
            {
                PickUpItem(nearbyItem);
            }
            else if (carriedItem != null)
            {
                DropItem();
            }
        }
    }

    private void PickUpItem(ICarryable item)
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ScrapPileInteractSFX, transform.position, 2f);
        carriedItem = item;
        nearbyItem = null;

        Transform t = item.Transform;
        t.SetParent(carryPoint);
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;

        if (item is ScrapCurrency scrap) scrap.ShrinkForCarry();
        else if (item is Cheese cheese) cheese.ShrinkForCarry();

        Debug.Log("Mouse picked up: " + t.name);
    }

    private void DropItem()
    {
        if (carriedItem == null) return;

        AudioManager.Instance.PlaySFX(AudioManager.Instance.ScrapPileInteractSFX, transform.position, 2f);

        Vector3 dropPosition = transform.position + transform.forward * 1f;

        Transform t = carriedItem.Transform;
        t.SetParent(null);
        t.localScale = Vector3.one;

        carriedItem.Drop(dropPosition);

        if (animator != null)
        {
            animator.SetTrigger("Interact");
        }

        Debug.Log("Mouse dropped: " + t.name);
        carriedItem = null;
    }

    public bool HasItem() => carriedItem != null;
    public ICarryable GetCarriedItem() => carriedItem;
    public void RemoveCarriedItem() => carriedItem = null;
}