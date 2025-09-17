using UnityEngine;

/*
 * Default bullet emitter; shoots bullets in a straight line.
 */
public class DefaultBulletEmitter : MonoBehaviour, IBulletEmitter
{
    [SerializeField] private GameObject bulletSource;
    public GameObject BulletSource 
    { 
        get => bulletSource; 
        set => bulletSource = value; 
    }
    public IOffense Owner { get; set; }
    
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
        Instantiate(BulletSource, transform.position, transform.rotation);
    }
}