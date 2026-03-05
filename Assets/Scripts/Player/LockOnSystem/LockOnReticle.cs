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
    private Canvas canvas;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        reticleImage = GetComponent<Image>();
        canvas = GetComponentInParent<Canvas>();

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
            reticleImage.enabled = false;
            return;
        }

        Vector3 screenPos = CameraManager.Cam.WorldToScreenPoint(target.transform.position);

        if (screenPos.z <= 0f)
        {
            reticleImage.enabled = false;
            return;
        }

        reticleImage.enabled = true;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : CameraManager.Cam,
            out Vector2 localPos
        );

        rectTransform.anchoredPosition = localPos;
    }
}