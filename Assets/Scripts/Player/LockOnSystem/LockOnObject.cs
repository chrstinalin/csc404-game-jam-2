using UnityEngine;
using UnityEngine.UI;

public class LockOnObject : MonoBehaviour
{
    public enum LockOnType
    {
        Enemy,
        Item,
        ZoomInItem
    }

    public string displayName;
    public string description;
    private Outline outline;
    public Sprite sprite;

    public LockOnType Type;

    void Start()
    {
        outline = GetComponent<Outline>();
    }

    void Update()
    {
        if (Type == LockOnType.Enemy)
        {
            UpdateOutlineBasedOnDistance();
        }
    }

    public float GetLockOnRequiredDistance()
    {
        switch (Type)
        {
            case LockOnType.Enemy:
                return Config.ATTACK_RANGE;
            case LockOnType.Item:
            case LockOnType.ZoomInItem:
                return Mathf.Infinity;
            default:
                return Mathf.Infinity;
        }
    }

    private void UpdateOutlineBasedOnDistance()
    {
        if (outline == null || PlayerMech.Instance == null)
            return;

        float distance = (transform.position - PlayerMech.Instance.transform.position).magnitude;
        float requiredDistance = GetLockOnRequiredDistance();

        if (distance > requiredDistance)
        {
            UpdateObjectOutline("Red", 1f);
        }
        else
        {
            UpdateObjectOutline("White", 2f);
        }
    }

    public void UpdateObjectOutline(string color, float thickness)
    {
        if (outline == null)
            return;

        Color col;
        if (ColorUtility.TryParseHtmlString(color, out col))
            outline.OutlineColor = col;

        outline.OutlineWidth = thickness;
    }
}