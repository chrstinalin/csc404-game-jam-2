using System;
using UnityEngine;
using UnityEngine.UI;

public class LockOnReticle : MonoBehaviour
{
    [NonSerialized] public CameraManager CameraManager;
    private LockOnManager lockOnManager;
    private LockOnObject target = null;
    private Canvas canvas;

    public RectTransform inRangeReticle;
    public RectTransform outOfRangeReticle;

    public float baseScale = 1f;

    public float inRangeScaleMultiplier = 1f;
    public float outOfRangeScaleMultiplier = 1f;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();

        if (inRangeReticle != null)
            inRangeReticle.gameObject.SetActive(false);
        if (outOfRangeReticle != null)
            outOfRangeReticle.gameObject.SetActive(false);
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
            if (inRangeReticle != null) inRangeReticle.gameObject.SetActive(false);
            if (outOfRangeReticle != null) outOfRangeReticle.gameObject.SetActive(false);
            return;
        }

        Vector3 targetCenter = target.GetCenterPoint().position;
        Vector3 screenPos = CameraManager.Cam.WorldToScreenPoint(targetCenter);

        if (screenPos.z <= 0f)
        {
            if (inRangeReticle != null) inRangeReticle.gameObject.SetActive(false);
            if (outOfRangeReticle != null) outOfRangeReticle.gameObject.SetActive(false);
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : CameraManager.Cam,
            out Vector2 localPos
        );

        if (lockOnManager.IsCurrentTargetInRange())
        {
            if (inRangeReticle != null)
            {
                inRangeReticle.gameObject.SetActive(true);
                inRangeReticle.anchoredPosition = localPos;
                inRangeReticle.localScale = Vector3.one * baseScale * inRangeScaleMultiplier;
            }
            if (outOfRangeReticle != null) outOfRangeReticle.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("out of range");
            if (outOfRangeReticle != null)
            {
                outOfRangeReticle.gameObject.SetActive(true);
                outOfRangeReticle.anchoredPosition = localPos;
                outOfRangeReticle.localScale = Vector3.one * baseScale * outOfRangeScaleMultiplier;
            }
            if (inRangeReticle != null) inRangeReticle.gameObject.SetActive(false);
        }
    }
}