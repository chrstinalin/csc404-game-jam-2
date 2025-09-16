using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int currHealth;
    
    [SerializeField] public UnityEvent<int> onHealthChanged;
    [SerializeField] public UnityEvent onDeath;

    public int GetCurrHealth() => currHealth;
    public int GetMaxHealth() => maxHealth;
    public bool IsAlive() => currHealth > 0;
    
    void Start()
    {
        currHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        int damageAmount = damage;
        currHealth -= damageAmount;
        
        onHealthChanged.Invoke(currHealth);
    
        if (currHealth <= 0)
        {
            onDeath.Invoke();
        }
    }
    
    public void Heal(int healAmount)
    {
        currHealth += healAmount;
        if (currHealth > maxHealth)
        {
            currHealth = maxHealth;
        }
        onHealthChanged.Invoke(currHealth);
    }
}