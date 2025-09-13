using UnityEngine;

public abstract class InventoryManager : MonoBehaviour
{
    public virtual void AddItem(CollectibleItem item) { }

    public virtual void AddItem(ItemData data) { }
}
