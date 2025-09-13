using UnityEngine;

public class MouseInventoryManager : InventoryManager
{
    public ItemData currentItem = null;

    public override void AddItem(CollectibleItem item)
    {
        currentItem = item.itemData;
        Debug.Log("Item picked up by mouse");
    }
}