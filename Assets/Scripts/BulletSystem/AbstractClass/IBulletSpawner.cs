using UnityEngine;

public interface IBulletSpawner
{
    GameObject Source { get; set; }
    int FiringRate { get; set; }
    int NumBullets { get; set; }
    
    void Fire();
}
