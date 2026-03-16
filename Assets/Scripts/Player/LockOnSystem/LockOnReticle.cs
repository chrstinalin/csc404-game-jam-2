using System;
using UnityEngine;
using UnityEngine.UI;

public class LockOnReticle : MonoBehaviour
{
    [NonSerialized] public CameraManager CameraManager;
    private LockOnManager lockOnManager;
    private LockOnObject target = null;
    private RectTransform rectTransform;
    private Image reticleImage;
    public Sprite inRangeSprite;
    public Sprite outOfRangeSprite;
    private Canvas canvas;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        reticleImage = GetComponent<Image>();
        canvas = GetComponentInParent<Canvas>();

        if (reticleImage != null)
            reticleImage.enabled = false;
    }

    void Start()
    {
        CameraManager = CameraManager.Instance;
        lockOnManager = LockOnManager.Instance;
    }

    void Update()
    {
        if (lockOnManager == null)
            return;

        target = lockOnManager.GetCurrentTarget();

        if (!lockOnManager.IsLockedOn || target == null)
        {
            if (reticleImage != null)
                reticleImage.enabled = false;
            return;
        }

        Vector3 screenPos = CameraManager.Cam.WorldToScreenPoint(target.transform.position);

        if (screenPos.z <= 0f)
        {
            if (reticleImage != null)
                reticleImage.enabled = false;
            return;
        }

        if (reticleImage != null)
        {
            reticleImage.enabled = true;
            reticleImage.sprite = lockOnManager.IsCurrentTargetInRange() ? inRangeSprite : outOfRangeSprite;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : CameraManager.Cam,
            out Vector2 localPos
        );

        rectTransform.anchoredPosition = localPos;
    }
}