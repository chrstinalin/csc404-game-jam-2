using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class InventoryUIManager : MonoBehaviour
{
    public MouseInventoryManager mouseInventory;
    public Image mouseItemImage;

    public MechaInventoryManager mechaInventory;
    public TMP_Text mechaInventoryText;

    private void Update()
    {
        UpdateMouseUI();
        UpdateMechaUI();
    }

    private void UpdateMouseUI()
    {
        if (mouseInventory == null || mouseItemImage == null)
            return;

        if (mouseInventory.items.Count > 0 && mouseInventory.items[0].icon != null)
        {
            mouseItemImage.sprite = mouseInventory.items[0].icon;
            mouseItemImage.enabled = true;
        }
        else
        {
            mouseItemImage.sprite = null;
            mouseItemImage.enabled = false;
        }
    }

    private void UpdateMechaUI()
    {
        if (mechaInventory == null || mechaInventoryText == null)
            return;

        mechaInventoryText.text = BuildMechaInventoryString();
    }

    private string BuildMechaInventoryString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Dreadnought Killer's Inventory:");

        if (mechaInventory.items != null && mechaInventory.items.Count > 0)
        {
            foreach (var item in mechaInventory.items)
            {
                sb.AppendLine("- " + item.itemName);
            }
        }

        return sb.ToString();
    }
}
