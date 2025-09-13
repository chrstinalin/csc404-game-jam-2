using UnityEngine;

public class MechaPickup : MonoBehaviour
{
    [Header("Inventory")]
    public MechaInventoryManager inventory;

    private void OnTriggerEnter(Collider other)
    {
        MouseInventoryManager mouseInventory = other.GetComponentInParent<MouseInventoryManager>();
        if (mouseInventory != null && mouseInventory.currentItem != null)
        {
            inventory.AddItem(mouseInventory.currentItem);
            mouseInventory.currentItem = null;
            Debug.Log("Mecha picked up item from Mouse: " + other.name);
        }
    }
}
