using UnityEngine;
using FMODUnity;
public class DestroyableWall : MonoBehaviour
{
    [SerializeField] public EventReference WallBreakSFX;
    private DamageReceiver DamageReceiver;
    private Health Health;
    
    void Awake()
    {
        Health = gameObject.GetComponent<Health>();
        Health.onDeath.AddListener(OnWallDestroyed);
        DamageReceiver = gameObject.AddComponent<DamageReceiver>();
        DamageReceiver.onTakeDamage.AddListener(TakeDamage);
    }
    
    public void TakeDamage(int damage)
    {
        if (Health != null)
        {
            Health.TakeDamage(damage);
        }
    }

    void OnWallDestroyed()
    {
        Debug.Log($"{gameObject.name} has been destroyed!");
        AudioManager.Instance.PlaySFX(WallBreakSFX, transform.position);
        Destroy(gameObject);
    }
}