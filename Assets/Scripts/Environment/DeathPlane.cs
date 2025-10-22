using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DeathPlane : MonoBehaviour
{
    private void Awake()
    {
        var col = GetComponent<BoxCollider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == PlayerMech.Instance.gameObject ||
            other.gameObject == PlayerMouse.Instance.gameObject)
        {
            CheckpointManager.Instance.RespawnCharacters();
        }
    }
}
