using UnityEngine;

public class TopTrigger : MonoBehaviour
{
    [HideInInspector] public bool mouseOnTop = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == PlayerMouse.Instance.gameObject)
        {
            mouseOnTop = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == PlayerMouse.Instance.gameObject)
        {
            mouseOnTop = false;
        }
    }
}
