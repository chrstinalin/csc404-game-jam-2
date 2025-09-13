using UnityEngine;
using UnityEngine.Events;

public class DamageReceiver : MonoBehaviour
{
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private UnityEvent<float> onTakeDamage;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            onTakeDamage.Invoke(damageAmount);
        }
    }
}
