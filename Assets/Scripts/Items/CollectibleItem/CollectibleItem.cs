using UnityEngine;

public abstract class CollectibleItem : MonoBehaviour
{
    public ItemData itemData;
    public virtual void OnPickUp()
    {
        Destroy(gameObject);
    }
}