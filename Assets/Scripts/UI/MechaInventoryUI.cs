using UnityEngine;
using TMPro;
using System.Text;

public class MechaInventoryDisplay : MonoBehaviour
{
    [Header("References")]
    public MechaInventoryManager mechaInventory;
    public TMP_Text inventoryText; // Assign your TMP Text object in Inspector

    private void Update()
    {
        if (inventoryText != null && mechaInventory != null)
        {
            inventoryText.text = BuildInventoryString();
        }
    }

    private string BuildInventoryString()
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
        else
        {
            sb.AppendLine("- Empty");
        }

        return sb.ToString();
    }
}
