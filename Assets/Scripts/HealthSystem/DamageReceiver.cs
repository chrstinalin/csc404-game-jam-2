using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DamageEvent : UnityEvent<int> { }

[System.Serializable]
public class DamageEventWithSource : UnityEvent<int, GameObject> { }

public class DamageReceiver : MonoBehaviour
{
    public DamageEvent onTakeDamage = new DamageEvent();
    public DamageEventWithSource onTakeDamageWithSource = new DamageEventWithSource();

    public void ReceiveDamage(int damage, GameObject source)
    {
        if (!gameObject.activeSelf) return;
        
        if (onTakeDamage != null)
        {
            onTakeDamage.Invoke(damage);
        }
        
        if (onTakeDamageWithSource != null)
        {
            onTakeDamageWithSource.Invoke(damage, source);
        }
    }
}