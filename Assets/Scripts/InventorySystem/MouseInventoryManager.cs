using UnityEngine;
using System.Collections;

public class MouseInventoryManager : MonoBehaviour
{
    private ICarryable nearbyItem; 
    [HideInInspector] public ICarryable carriedItem; 
    private Transform carryPoint;
    private Animator animator;

    private bool isBusy = false; // prevents spam during delay

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
        if (!playerControllingMouse || isBusy) return;

        if (Input.GetButtonDown("Interact"))
        {
            if (carriedItem == null && nearbyItem != null)
            {
                StartCoroutine(PickUpRoutine(nearbyItem));
            }
            else if (carriedItem != null)
            {
                StartCoroutine(DropRoutine());
            }
        }
    }

    private IEnumerator PickUpRoutine(ICarryable item)
    {
        isBusy = true;

        // 🔹 Play animation immediately
        if (animator != null)
            animator.SetTrigger("Interact");

        // 🔹 Optional: play sound immediately
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ScrapPileInteractSFX, transform.position, 2f);

        // 🔹 Wait for animation timing
        yield return new WaitForSeconds(0.3f);

        // 🔹 Actual pickup happens AFTER delay
        carriedItem = item;
        nearbyItem = null;

        Transform t = item.Transform;
        t.SetParent(carryPoint);
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;

        if (item is ScrapCurrency scrap) scrap.ShrinkForCarry();
        else if (item is Cheese cheese) cheese.ShrinkForCarry();

        Debug.Log("Mouse picked up: " + t.name);

        isBusy = false;
    }

    private IEnumerator DropRoutine()
    {
        if (carriedItem == null) yield break;

        isBusy = true;

        // 🔹 Play animation first
        if (animator != null)
            animator.SetTrigger("Interact");

        AudioManager.Instance.PlaySFX(AudioManager.Instance.ScrapPileInteractSFX, transform.position, 2f);

        yield return new WaitForSeconds(0.3f);

        Vector3 dropPosition = transform.position + transform.forward * 1f;

        Transform t = carriedItem.Transform;
        t.SetParent(null);

        carriedItem.Drop(dropPosition);

        Debug.Log("Mouse dropped: " + t.name);

        carriedItem = null;

        isBusy = false;
    }

    public bool HasItem() => carriedItem != null;
    public ICarryable GetCarriedItem() => carriedItem;
    public void RemoveCarriedItem() => carriedItem = null;
}