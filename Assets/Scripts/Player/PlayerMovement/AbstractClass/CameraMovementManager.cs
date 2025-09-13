using UnityEngine;

public abstract class CameraMovementManager : MonoBehaviour
{
    public float zoom;
    public float zoomMultiplier = 4f;
    public float minZoom = 1f;
    public float maxZoom = 10f;
    public float velocity = 0f;
    public float smoothTime = 0.25f;

    public abstract void SetFollowEntity(GameObject entity, float? newZoom);
    public abstract void UpdateCamera();
    public abstract void PanTo(float zoomSize);

    public abstract void SetMaxZoom(float maxZoom);
}
