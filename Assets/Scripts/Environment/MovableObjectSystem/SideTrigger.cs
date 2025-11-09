using UnityEngine;

public class SideTrigger : MonoBehaviour
{
    public CardinalDirection side;

    [HideInInspector] public bool mechInRange = false;
    [HideInInspector] public bool blocked = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == PlayerMech.Instance.gameObject) mechInRange = true;
        else blocked = true;
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == PlayerMech.Instance.gameObject) mechInRange = true;
        else blocked = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == PlayerMech.Instance.gameObject) mechInRange = false;
        else blocked = false;
    }

    public bool CanPush() => mechInRange;
}
