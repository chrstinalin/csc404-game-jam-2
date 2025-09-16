using UnityEngine;

/*
 * Default bullet class; shoots straight forward.
 */
public class DefaultBullet : Bullet
{
    protected override void HandleMovement()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}
