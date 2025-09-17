using UnityEngine;

public class PlayerMech : MonoBehaviour
{
    private Health _health;
    [SerializeField] MovementManager movementManager;
    
    void Start()
    {
        _health = GetComponent<Health>();
        _health.onDeath.AddListener(OnDeath);
    }

    public void OnHealthChanged(int damage)
    {
        if (damage < 0)
        {
            Debug.Log("$Healed {-damage} health!");
        }
        else
        {
            if (movementManager != null && !movementManager.IsMouseActive)
            {
                Debug.Log("Mech took damage! Ejecting mouse...");
                movementManager.ToggleMouse(true);
            }
        }
    }
    
    public void OnDeath()
    {
        Debug.Log("Mech Died. Respawning...");
        transform.position = new Vector3(0, 1, 0);
        _health.Heal(_health.GetMaxHealth());
    }
}