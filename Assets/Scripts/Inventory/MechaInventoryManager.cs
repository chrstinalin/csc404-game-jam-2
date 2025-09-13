using System.Collections.Generic;
using UnityEngine;

public class MechaInventoryManager : InventoryManager
{
    public List<ItemData> items = new List<ItemData>();

    public override void AddItem(ItemData itemData)
    {
        items.Add(itemData);
        Debug.Log("Item added to mecha inventory");
    }
}