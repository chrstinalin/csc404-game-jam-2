using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Reflection;
using System.Collections.Generic;

public class CameraLockTrigger : MonoBehaviour
{
    [SerializeField] private Vector3 lockedCameraPosition;
    [SerializeField] private Vector3 lockedCameraRotation;

    private Dictionary<PSXRenderFeature, (int width, int height)> originalValues
        = new Dictionary<PSXRenderFeature, (int, int)>();

    private bool hasCachedOriginals = false;

    private void Awake()
    {
        CacheOriginalValues();
        ApplyResolution(640, 480);
    }

    private void OnDisable()
    {
        RestorePSXResolution();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("MousePlayerEntity")) return;

        Quaternion rotation = Quaternion.Euler(lockedCameraRotation);
        CameraManager.Instance.SetCameraLock(lockedCameraPosition, true, rotation);

        ApplyResolution(1920, 1440);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("MousePlayerEntity")) return;

        CameraManager.Instance.SetCameraLock(Vector3.zero, false);

        ApplyResolution(640, 480);
    }


    private void CacheOriginalValues()
    {
        if (hasCachedOriginals) return;

        var psxFeatures = GetPSXFeatures();
        if (psxFeatures == null) return;

        foreach (var psx in psxFeatures)
        {
            originalValues[psx] = (psx.settings.targetWidth, psx.settings.targetHeight);
        }

        hasCachedOriginals = true;
    }

    private void ApplyResolution(int width, int height)
    {
        var psxFeatures = GetPSXFeatures();
        if (psxFeatures == null) return;

        foreach (var psx in psxFeatures)
        {
            psx.settings.targetWidth = width;
            psx.settings.targetHeight = height;
        }
    }

    private void RestorePSXResolution()
    {
        if (!hasCachedOriginals) return;

        foreach (var kvp in originalValues)
        {
            var psx = kvp.Key;
            var (width, height) = kvp.Value;

            if (psx != null)
            {
                psx.settings.targetWidth = width;
                psx.settings.targetHeight = height;
            }
        }
    }

    private List<PSXRenderFeature> GetPSXFeatures()
    {
        var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset == null) return null;

        var renderer = urpAsset.scriptableRenderer;
        if (renderer == null) return null;

        var field = typeof(ScriptableRenderer)
            .GetField("m_RendererFeatures", BindingFlags.NonPublic | BindingFlags.Instance);

        var features = field?.GetValue(renderer) as List<ScriptableRendererFeature>;
        if (features == null) return null;

        List<PSXRenderFeature> result = new List<PSXRenderFeature>();

        foreach (var feature in features)
        {
            if (feature is PSXRenderFeature psx)
                result.Add(psx);
        }

        return result;
    }
}