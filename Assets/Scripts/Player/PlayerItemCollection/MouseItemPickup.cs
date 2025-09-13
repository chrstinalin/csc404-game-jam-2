using UnityEngine;

public class MousePickup : MonoBehaviour
{
    [Header("Inventory")]
    public MouseInventoryManager inventory;

    private CollectibleItem nearbyCollectible;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Collectible"))
        {
            CollectibleItem collectible = other.GetComponent<CollectibleItem>();
            if (collectible != null)
            {
                nearbyCollectible = collectible;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Collectible"))
        {
            CollectibleItem collectible = other.GetComponent<CollectibleItem>();
            if (collectible != null && collectible == nearbyCollectible)
            {
                nearbyCollectible = null;
            }
        }
    }

    private void Update()
    {
        if (nearbyCollectible != null && Input.GetButtonDown("PickupItem"))
        {
            if (inventory != null)
            {
                inventory.AddItem(nearbyCollectible);
                nearbyCollectible.OnPickUp();
                nearbyCollectible = null;
            }
        }
    }
}
