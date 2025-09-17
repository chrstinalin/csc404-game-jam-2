using UnityEngine;

public interface IBulletEmitter
{
    public IOffense Owner { get; set; }
    public GameObject BulletSource { get; set; }
    public abstract void Fire();
}