using UnityEngine;
using System.Collections;

public class MouseInventoryManager : MonoBehaviour
{
    private ScrapCurrency nearbyItem;
    [HideInInspector] public ScrapCurrency carriedItem;
    private Transform carryPoint;
    private Animator animator;

    private float animationDuration = 0.4f;

    private void Awake()
    {
        carryPoint = transform.Find("CarryPoint");
        if (carryPoint == null)
            Debug.LogError(
                "A Transform named CarryPoint where the scrap currency " +
                "appears when carried must be a child of PlayerMouse"
            );

        animator = GetComponentInChildren<Animator>();
        if (animator == null)
            Debug.LogError("No Animator found in children of PlayerMouse");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (MovementManager.Instance == null || !MovementManager.Instance.IsMouseActive)
            return;
            
        var item = other.GetComponent<ScrapCurrency>();

        if (item != null && item.GetComponent<Health>() == null)
        {
            nearbyItem = item;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var item = other.GetComponent<ScrapCurrency>();
        if (item != null && item == nearbyItem)
        {
            nearbyItem = null;
        }
    }

    private void Update()
    {
        bool playerControllingMouse = MovementManager.Instance != null && MovementManager.Instance.IsMouseActive;
        if (!playerControllingMouse)
        {
            return;
        }

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

    private void PickUpItem(ScrapCurrency item)
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ScrapPileInteractSFX, transform.position, 2f);
        carriedItem = item;
        nearbyItem = null;

        item.transform.SetParent(carryPoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        item.transform.localScale = Vector3.one * 0.5f;

        Debug.Log("Mouse picked up: " + item.name);
    }

    private void DropItem()
    {
        if (carriedItem == null) return;
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ScrapPileInteractSFX, transform.position, 2f);

        Vector3 dropPosition = transform.position + transform.forward * 1f;

        carriedItem.transform.SetParent(null);
        carriedItem.transform.localScale = Vector3.one;
        carriedItem.Drop(dropPosition);

        if (animator != null)
        {
            animator.SetTrigger("Interact");
        }

        Debug.Log("Mouse dropped: " + carriedItem.name);
        carriedItem = null;
    }

    public bool HasItem() => carriedItem != null;
    public ScrapCurrency GetCarriedItem() => carriedItem;
    public void RemoveCarriedItem() => carriedItem = null;
}
