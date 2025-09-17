using UnityEngine;
using UnityEngine.Events;

public class DamageReceiver : MonoBehaviour
{
    [SerializeField] private UnityEvent<int> onTakeDamage;
    
    void OnTriggerEnter(Collider other)
    {
        var bullet = other.GetComponent<Bullet>();
        if (bullet == null) return;
        onTakeDamage.Invoke(bullet.damage);
        Destroy(other.gameObject);
    }
}