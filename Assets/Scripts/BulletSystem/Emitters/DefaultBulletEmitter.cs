using System;
using UnityEngine;

/*
 * Default bullet emitter; shoots bullets in a straight line.
 */
public class DefaultBulletEmitter : MonoBehaviour, IBulletSpawner
{
    public IOffense Owner;
    public GameObject bulletSource;
    public int firingRate = 1;
    private float _timer;
    public int numBullets = 1;

    void Start()
    {
        Owner = GetComponent<IOffense>() ?? GetComponentInParent<IOffense>();
    }
    
    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= firingRate && Owner != null && Owner.isAttack())
        {
            Fire();
            _timer = 0;
        }
    }
    
    public virtual void Fire()
    {
        Instantiate(bulletSource, transform.position, transform.rotation);
    }
}