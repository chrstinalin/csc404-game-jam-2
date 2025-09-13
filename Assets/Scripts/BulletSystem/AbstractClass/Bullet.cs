using UnityEngine;

public abstract class Bullet : MonoBehaviour
{

    public GameObject source;
    public int damage = 1;
    public int speed = 10;
    public int lifetime = 1000;
    
    void Update()
    {
        lifetime -= 1;
        if (lifetime <= 0)
        {
            Destroy(gameObject);
        }
    }
}
