using UnityEngine;

public class MouseInventoryManager : InventoryAbstractManager
{
    public ItemData currentItem = null;

    public override void AddItem(ItemData item)
    {
        currentItem = item;
        Debug.Log("Item picked up by Peanut");
    }
}