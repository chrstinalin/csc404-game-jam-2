using UnityEngine;
using UnityEngine.Events;

public class DamageReceiver : MonoBehaviour
{
    [SerializeField] private UnityEvent<float> onTakeDamage;

    void OnTriggerEnter(Collider other)
    {
        Bullet bullet = other.GetComponent<Bullet>();
        if (bullet != null)
        {
            onTakeDamage.Invoke(bullet.damage);
            Destroy(other.gameObject);
        }
    }
}
