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
        if (item != null)
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
            return;  // Early exit if mouse is not being controlled
        }

        if (Input.GetButtonDown("Interact"))
        {
            if (carriedItem == null && nearbyItem != null)
            {
                StartCoroutine(PickUpItem(nearbyItem));
            }
            else if (carriedItem != null)
            {
                DropItem();
            }
        }
    }

    private IEnumerator PickUpItem(ScrapCurrency item)
    {
        if (MovementManager.Instance != null) MovementManager.Instance.isLockedMovement = true;
        if (animator != null)
        {
            animator.SetTrigger("Interact");
        }
        yield return new WaitForSeconds(animationDuration);

        carriedItem = item;
        nearbyItem = null;

        item.transform.SetParent(carryPoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        item.transform.localScale = Vector3.one * 0.5f;

        Debug.Log("Mouse picked up: " + item.name);

        if (MovementManager.Instance != null) MovementManager.Instance.isLockedMovement = false;
    }


    private void DropItem()
    {
        if (carriedItem == null) return;

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
