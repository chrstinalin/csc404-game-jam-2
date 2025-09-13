using UnityEngine;

public class PlayerItemPickupManager : MonoBehaviour
{
    public GameObject mouse;
    public GameObject mecha;

    public MousePickup mousePickup;
    public MechaPickup mechaPickup;

    private void Awake()
    {
        if (mouse != null)
        {
            mousePickup = mouse.GetComponent<MousePickup>();
        }

        if (mecha != null)
        {
            mechaPickup = mecha.GetComponent<MechaPickup>();
        }
    }
}
