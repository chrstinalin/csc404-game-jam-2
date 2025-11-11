using UnityEngine;
using UnityEngine.UI;

public class OffScreenIndicator : MonoBehaviour
{
    private Transform mouse;
    private Transform mech;
    public RectTransform indicatorUI;
    public Canvas canvas;
    
    private float edgeBuffer = 50f;
    private bool rotateIndicator = true;
    
    private Camera cam;
    private RectTransform canvasRect;
    private Transform currentPlayer;
    private Transform otherPlayer;

    void Start()
    {
        mouse = PlayerMouse.Instance.gameObject.transform;
        mech = PlayerMech.Instance.gameObject.transform;
        cam = CameraManager.Instance.Cam;
        canvasRect = canvas.GetComponent<RectTransform>();
        indicatorUI.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (cam == null)
        {
            cam = CameraManager.Instance?.Cam;
            if (cam == null) return;
        }

        GameObject followEntity = CameraManager.Instance.FollowEntity;
        if (followEntity == null) return;

        if (followEntity.transform == mouse)
        {
            currentPlayer = mouse;
            otherPlayer = mech;
        }
        else if (followEntity.transform == mech)
        {
            currentPlayer = mech;
            otherPlayer = mouse;
        }
        else
        {
            return;
        }

        UpdateIndicator();
    }

    void UpdateIndicator()
    {
        if (cam == null || otherPlayer == null) return;
        
        Vector3 screenPos = cam.WorldToViewportPoint(otherPlayer.position);
        
        bool isOffScreen = screenPos.z < 0 || 
                        screenPos.x < 0 || screenPos.x > 1 || 
                        screenPos.y < 0 || screenPos.y > 1;

        if (isOffScreen)
        {
            indicatorUI.gameObject.SetActive(true);
            
            if (screenPos.z < 0) screenPos *= -1;

            Vector2 screenPoint = new Vector2(
                screenPos.x * Screen.width,
                screenPos.y * Screen.height
            );

            Vector2 canvasPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPoint,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam,
                out canvasPos
            );

            float maxX = (canvasRect.rect.width / 2) - edgeBuffer;
            float maxY = (canvasRect.rect.height / 2) - edgeBuffer;
            
            canvasPos = new Vector2(
                Mathf.Clamp(canvasPos.x, -maxX, maxX),
                Mathf.Clamp(canvasPos.y, -maxY, maxY)
            );

            indicatorUI.anchoredPosition = canvasPos;

            if (rotateIndicator)
            {
                float angleInDegrees = Mathf.Atan2(canvasPos.y, canvasPos.x) * Mathf.Rad2Deg;
                indicatorUI.rotation = Quaternion.Euler(0, 0, angleInDegrees - 90);
            }
        }
        else
        {
            indicatorUI.gameObject.SetActive(false);
        }
    }
}