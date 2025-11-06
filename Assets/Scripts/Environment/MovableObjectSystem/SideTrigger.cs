using UnityEngine;

public class SideTrigger : MonoBehaviour
{
    public CardinalDirection side;
    public GameObject player;

    [HideInInspector] public bool playerInRange = false;
    [HideInInspector] public bool blocked = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player) playerInRange = true;
        else blocked = true;
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == player) playerInRange = true;
        else blocked = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player) playerInRange = false;
        else blocked = false;
    }

    public bool CanPush() => playerInRange;
}
