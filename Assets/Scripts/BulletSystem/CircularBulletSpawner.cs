using UnityEngine;

public class CircularBulletSpawner : BaseBulletSpawner
{
    public override void Fire()
    {
        for (float i = 0; i < numBullets; i++)
        {
            float angle = i * 2f * Mathf.PI / numBullets;
        
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            Vector3 spawnPosition = transform.position + direction * 2f;
        
            Quaternion bulletRotation = Quaternion.LookRotation(direction);
        
            Instantiate(source, spawnPosition, bulletRotation);
        }
    }
}
