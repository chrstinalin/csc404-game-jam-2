using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class Laser : MonoBehaviour
{
    private void Awake()
    {
        var col = GetComponent<CapsuleCollider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignore trigger colliders
        if (other.isTrigger)
            return;

        GameObject root = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;

        if (root == PlayerMech.Instance.gameObject || root == PlayerMouse.Instance.gameObject)
        {
            CheckpointManager.Instance.RespawnCharacters();

            PlayerMouse.Instance.Health.Heal(PlayerMouse.Instance.Health.GetMaxHealth());
            PlayerMech.Instance.Health.Heal(PlayerMech.Instance.Health.GetMaxHealth());
        }
    }
}
