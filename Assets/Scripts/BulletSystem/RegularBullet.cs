using UnityEngine;

public class RegularBullet : Bullet
{
    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }
}