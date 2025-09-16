using UnityEngine;

public abstract class CollectibleItem : MonoBehaviour
{
    public ItemData itemData;

    public void OnPickUp()
    {
        Destroy(gameObject);
    }
}
