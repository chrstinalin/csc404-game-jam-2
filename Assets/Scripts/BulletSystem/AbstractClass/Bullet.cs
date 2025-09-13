using UnityEngine;

public abstract class Bullet : MonoBehaviour
{

    public Enemy source;
    [SerializeField] protected int damage = 1;
}
