using System.Collections.Generic;
using UnityEngine;

public class MechaInventoryManager : InventoryAbstractManager
{
    public List<ItemData> items = new List<ItemData>();

    public override void AddItem(ItemData itemData)
    {
        items.Add(itemData);
        Debug.Log("Item picked up by Dreadnought Killer");
    }
}