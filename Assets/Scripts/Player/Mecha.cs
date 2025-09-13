using UnityEngine;

public class Mecha : MonoBehaviour
{
    [SerializeField] private MovementManager movementManager;
    
    public void TakeDamage(float damage)
    {
        Debug.Log($"Mecha took {damage} damage");
        if (movementManager != null)
        {
            Debug.Log("Enabling mouse control");
            movementManager.ToggleMouse(true);
        }
    }
}
