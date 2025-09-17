using UnityEngine;

public class PlayerMouse : MonoBehaviour
{
    private Health _health;
    
    void Start()
    {
        _health = GetComponent<Health>();
        _health.onDeath.AddListener(OnDeath);
    }
    
    public void OnDeath()
    {
        Debug.Log("Player Died. Respawning...");
        transform.position = new Vector3(0, 1, 0);
        _health.Heal(_health.GetMaxHealth());
    }
}