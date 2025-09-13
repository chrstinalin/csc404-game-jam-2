using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Items/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName = "Item";
    public Sprite icon;
}
