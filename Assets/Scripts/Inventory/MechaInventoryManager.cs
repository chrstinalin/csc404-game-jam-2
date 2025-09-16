using System.Collections.Generic;
using UnityEngine;

public class MechaInventoryManager : InventoryAbstractManager
{
    public GameObject Mouse;

    private void Awake()
    {
        if (items == null)
            items = new List<ItemData>();

        maxNumItems = 0; // No limit
    }

    private void Update()
    {
        if (Mouse == null) return;

        MouseInventoryManager mouseInventory = Mouse.GetComponent<MouseInventoryManager>();
        if (mouseInventory == null) return;

        // Condition 1: Mouse is inactive
        if (!Mouse.activeInHierarchy && mouseInventory.items.Count > 0)
        {
            TransferItem(mouseInventory);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Condition 2: Mouse collides with Mecha
        MouseInventoryManager mouseInventory = other.GetComponentInParent<MouseInventoryManager>();
        if (mouseInventory != null && mouseInventory.items.Count > 0)
        {
            TransferItem(mouseInventory);
        }
    }

    private void TransferItem(MouseInventoryManager mouseInventory)
    {
        AddItem(mouseInventory.items[0]);
        mouseInventory.RemoveItem(0);
        Debug.Log("Mecha picked up item from Mouse");
    }

    public override void AddItem(ItemData data)
    {
        items.Add(data);
        Debug.Log("Item picked up by Dreadnought Killer");
    }

    public override void RemoveItem(int index)
    {
        // Do Nothing
    }
}
