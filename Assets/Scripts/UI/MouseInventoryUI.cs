using UnityEngine;
using UnityEngine.UI;

public class MouseInventoryImage : MonoBehaviour
{
    [Header("References")]
    public MouseInventoryManager mouseInventory;
    public Image itemImage;                       

    private void Update()
    {
        if (mouseInventory != null && itemImage != null)
        {
            if (mouseInventory.currentItem != null && mouseInventory.currentItem.icon != null)
            {
                itemImage.sprite = mouseInventory.currentItem.icon;
                itemImage.enabled = true;
            }
            else
            {
                itemImage.sprite = null;
                itemImage.enabled = false;
            }
        }
    }
}
