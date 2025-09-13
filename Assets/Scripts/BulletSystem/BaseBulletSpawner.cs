using UnityEngine;

public class BaseBulletSpawner : MonoBehaviour, IBulletSpawner
{
    [SerializeField] public GameObject source;
    public int firingRate = 10;
    private float _timer = 0;
    public int numBullets = 7;
    
    public GameObject Source 
    { 
        get => source; 
        set => source = value; 
    }
    
    public int FiringRate 
    { 
        get => firingRate; 
        set => firingRate = value; 
    }
    
    public int NumBullets 
    { 
        get => numBullets; 
        set => numBullets = value; 
    }
    
    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= firingRate)
        {
            Fire();
            _timer = 0;
        }
    }
    
    public virtual void Fire()
    {
        if (source)
        {
            Instantiate(source, transform.position, Quaternion.identity);
        }
    }
}