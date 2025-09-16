using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Items/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName = "Item";
    public Sprite icon;
}