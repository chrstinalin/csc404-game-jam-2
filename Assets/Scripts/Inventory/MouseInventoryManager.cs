using UnityEngine;

public class MouseInventoryManager : InventoryAbstractManager
{
    private CollectibleItem nearbyCollectible;

    private void Awake()
    {
        if (items == null)
            items = new System.Collections.Generic.List<ItemData>();

        maxNumItems = 1;
    }

    private void OnTriggerEnter(Collider other)
    {
        CollectibleItem collectible = other.GetComponent<CollectibleItem>();
        if (collectible != null)
        {
            nearbyCollectible = collectible;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CollectibleItem collectible = other.GetComponent<CollectibleItem>();
        if (collectible != null && collectible == nearbyCollectible)
        {
            nearbyCollectible = null; 
        }
    }

    private void Update()
    {
        if (Input.GetButtonDown("DropItem"))
        {   
            // There is only 1 item in Peanut's inventory
            RemoveItem(0);
        }
        
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
            Debug.Log("Mouse inventory full! Cannot pick up another item.");
            return;
        }

        items.Add(item);
        Debug.Log("Item picked up by Peanut");
    }

    public override void RemoveItem(int index)
    {
        if (index < 0 || index >= items.Count)
        {
            return;
        }

        ItemData removedItem = items[index];
        items.RemoveAt(index);
        Debug.Log("Removed item: " + removedItem.name);
    }
}
