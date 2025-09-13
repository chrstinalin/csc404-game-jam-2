using UnityEngine;

public class Mecha : MonoBehaviour
{
    [SerializeField] private MovementManager movementManager;
    
    public void TakeDamage(float damage)
    {
        if (movementManager != null && !movementManager.IsMouseActive)
        {
            movementManager.ToggleMouse(true);
        }
    }
}
