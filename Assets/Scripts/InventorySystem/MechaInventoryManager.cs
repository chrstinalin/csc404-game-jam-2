using UnityEngine;

public class MechaInventoryManager : MonoBehaviour
{
    private Health mechHealth;
    private MouseInventoryManager mouseInventory;
    private PlayerMouse mouse;

    private void Start()
    {
        mouse = PlayerMouse.Instance;
        mechHealth = PlayerMech.Instance.GetComponent<Health>();

        if (mouse != null)
        {
            mouseInventory = mouse.GetComponent<MouseInventoryManager>();
        }
    }

    private void Update()
    {
        if (mouse == null) return;
        if (mouseInventory == null) return;

        if (mouseInventory.HasItem())
        {
            float distance = Vector3.Distance(mouse.transform.position, PlayerMech.Instance.transform.position);
            if (distance < Config.MECH_ENTER_DISTANCE)
            {
                TakeFromMouse(mouseInventory);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var otherInventory = other.GetComponentInParent<MouseInventoryManager>();
        if (otherInventory != null && otherInventory.HasItem())
        {
            TakeFromMouse(otherInventory);
        }
    }

    private void TakeFromMouse(MouseInventoryManager sourceInventory)
    {
        if (!sourceInventory.HasItem()) return;

        ScrapCurrency scrap = sourceInventory.GetCarriedItem();
        if (scrap != null)
        {
            if (mechHealth != null)
            {
                mechHealth.Heal(scrap.HPRestoreAmount);
            }

            Destroy(scrap.gameObject);
            sourceInventory.RemoveCarriedItem();
        }
    }
}
