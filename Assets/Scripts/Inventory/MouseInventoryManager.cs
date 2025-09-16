using UnityEngine;

public class MouseInventoryManager : InventoryAbstractManager
{
    public ItemData currentItem = null;
    private CollectibleItem nearbyCollectible;

    private void Awake()
    {
        // Initialize the abstract base fields
        if (items == null)
            items = new System.Collections.Generic.List<ItemData>();

        maxNumItems = 1;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Detect collectible items
        CollectibleItem collectible = other.GetComponent<CollectibleItem>();
        if (collectible != null)
        {
            nearbyCollectible = collectible;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Clear reference when leaving the trigger
        CollectibleItem collectible = other.GetComponent<CollectibleItem>();
        if (collectible != null && collectible == nearbyCollectible)
        {
            nearbyCollectible = null;
        }
    }

    private void Update()
    {
        // Pick up item when the player presses the PickupItem input
        if (nearbyCollectible != null && Input.GetButtonDown("PickupItem"))
        {
            AddItem(nearbyCollectible.itemData);
            nearbyCollectible.OnPickUp();
            nearbyCollectible = null;
        }
    }

    public override void AddItem(ItemData item)
    {
        if (items.Count >= maxNumItems)
        {
            Debug.LogWarning("Mouse inventory full! Cannot pick up another item.");
            return;
        }

        currentItem = item;
        items.Add(item);
        Debug.Log("Item picked up by Peanut");
    }
}
