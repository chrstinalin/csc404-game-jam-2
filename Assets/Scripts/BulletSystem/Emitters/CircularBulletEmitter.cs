using UnityEngine;

/*
 * Circular bullet emitter; shoots bullets in a circle around the emitter.
 */
public class CircularBulletEmitter : DefaultBulletEmitter
{
    public override void Fire()
    {
        for (float i = 0; i < numBullets; i++)
        {
            float angle = i * 2f * Mathf.PI / numBullets;

            Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            Vector3 spawnPosition = transform.position + direction * 2f;

            Quaternion bulletRotation = Quaternion.LookRotation(direction);

            Instantiate(BulletSource, spawnPosition, bulletRotation);
        }
    }
}