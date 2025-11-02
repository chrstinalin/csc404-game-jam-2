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
        bool isPlayer =
            root == PlayerMech.Instance.gameObject ||
            root == PlayerMouse.Instance.gameObject;

        if (!isPlayer)
            return;

        DamageReceiver damageReceiver = other.GetComponent<DamageReceiver>();
        if (damageReceiver == null)
            damageReceiver = other.GetComponentInParent<DamageReceiver>();

        if (damageReceiver != null)
        {
            damageReceiver.ReceiveDamage(int.MaxValue, gameObject);
        }
    }
}
