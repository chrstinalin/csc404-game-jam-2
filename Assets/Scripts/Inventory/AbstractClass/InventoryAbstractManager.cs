using System.Collections.Generic;
using UnityEngine;

public abstract class InventoryAbstractManager : MonoBehaviour
{
    public List<ItemData> items;
    public int maxNumItems;

    public abstract void AddItem(ItemData data);
    public abstract void RemoveItem(int index);
}
