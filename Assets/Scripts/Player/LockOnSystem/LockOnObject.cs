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

    public Transform centerPoint;

    void Start()
    {
        outline = GetComponent<Outline>();

        if (centerPoint == null)
        {
            centerPoint = new GameObject("AutoCenterPoint").transform;
            centerPoint.SetParent(transform, true);
            centerPoint.position = CalculateMeshCenter();
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

    public void UpdateObjectOutline(string color, float thickness)
    {
        if (outline == null)
            return;

        if (ColorUtility.TryParseHtmlString(color, out Color col))
            outline.OutlineColor = col;

        outline.OutlineWidth = thickness;
    }

    public Transform GetCenterPoint()
    {
        return centerPoint;
    }

    private Vector3 CalculateMeshCenter()
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        if (renderers.Length == 0)
            return transform.position;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return bounds.center;
    }
}