using System.Collections.Generic;
using UnityEngine;

public class MechaInventoryManager : InventoryAbstractManager
{
    private void Awake()
    {
        if (items == null)
            items = new List<ItemData>();

        // No upper limit
        maxNumItems = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        MouseInventoryManager mouseInventory = other.GetComponentInParent<MouseInventoryManager>();
        if (mouseInventory != null && mouseInventory.currentItem != null)
        {
            AddItem(mouseInventory.currentItem);
            mouseInventory.currentItem = null;
            Debug.Log("Mecha picked up item from Mouse: " + other.name);
        }
    }

    public override void AddItem(ItemData data)
    {
        items.Add(data);
        Debug.Log("Item picked up by Dreadnought Killer");
    }
}
