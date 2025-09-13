using UnityEngine;

public class Mouse : MonoBehaviour
{
    [SerializeField] private int health = 3;

    public int GetHealth()
    {
        return health;
    }
    
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Debug.Log("player ded");
            health = 3;
            transform.position = new Vector3(0, 1, 0);
        }
        else
        {
            Debug.Log($"Player took {damage} damage, remaining health: {health}");
        }
    }
}
