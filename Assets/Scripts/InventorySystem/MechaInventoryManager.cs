using UnityEngine;

public class MechaInventoryManager : MonoBehaviour
{
    private Health mechHealth;
    private MouseInventoryManager mouseInventory;
    private MovementManager movementManager;
    private PlayerMouse mouse;

    private void Start()
    {
        mouse = PlayerMouse.Instance;
        mechHealth = PlayerMech.Instance.GetComponent<Health>();

        if (mouse != null)
        {
            mouseInventory = mouse.GetComponent<MouseInventoryManager>();
        }
        movementManager = MovementManager.Instance;
    }

    private void Update()
    {
        if (mouse == null) return;
        if (mouseInventory == null) return;

        if (Input.GetButtonDown("Interact") && mouseInventory.HasItem() && movementManager.IsMouseActive)
        {
            float distance = Vector3.Distance(mouse.transform.position, PlayerMech.Instance.transform.position);
            if (distance < Config.MECH_ENTER_DISTANCE)
            {
                TakeFromMouse(mouseInventory);
            }
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
