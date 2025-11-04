using UnityEngine;
using System.Collections.Generic;

public class WallTransparencyController : MonoBehaviour
{
    [Header("Settings")]
    public float fadeDistance = 3f;       // Distance at which transparency starts
    public float fadeSpeed = 5f;          // How fast the fade happens
    [Range(0f, 1f)] public float minAlpha = 0.3f;  // Minimum alpha when faded

    [Header("Detection")]
    public LayerMask wallLayer;           // Only affect walls on this layer
    public Camera mainCamera;

    private readonly Dictionary<Renderer, float> originalAlphas = new Dictionary<Renderer, float>();

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        FadeNearbyWalls();
    }

    void FadeNearbyWalls()
    {
        // Find all colliders within the fade distance
        Collider[] walls = Physics.OverlapSphere(mainCamera.transform.position, fadeDistance, wallLayer);

        HashSet<Renderer> nearbyRenderers = new HashSet<Renderer>();
        foreach (var wall in walls)
        {
            Renderer rend = wall.GetComponent<Renderer>();
            if (rend)
            {
                nearbyRenderers.Add(rend);
                SetTransparency(rend, minAlpha);
            }
        }

        // Restore any walls that are no longer nearby
        List<Renderer> toRestore = new List<Renderer>(originalAlphas.Keys);
        foreach (var rend in toRestore)
        {
            if (!nearbyRenderers.Contains(rend))
                RestoreTransparency(rend);
        }
    }

    void SetTransparency(Renderer rend, float targetAlpha)
    {
        if (!originalAlphas.ContainsKey(rend))
            originalAlphas[rend] = rend.material.color.a;

        Color c = rend.material.color;
        float newAlpha = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * fadeSpeed);
        c.a = newAlpha;
        rend.material.color = c;

        SetMaterialToTransparent(rend.material);
    }

    void RestoreTransparency(Renderer rend)
    {
        if (!originalAlphas.ContainsKey(rend)) return;

        Color c = rend.material.color;
        float newAlpha = Mathf.Lerp(c.a, originalAlphas[rend], Time.deltaTime * fadeSpeed);
        c.a = newAlpha;
        rend.material.color = c;

        if (Mathf.Abs(c.a - originalAlphas[rend]) < 0.05f)
        {
            c.a = originalAlphas[rend];
            rend.material.color = c;
            SetMaterialToOpaque(rend.material);
            originalAlphas.Remove(rend);
        }
    }

    void SetMaterialToTransparent(Material mat)
    {
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }

    void SetMaterialToOpaque(Material mat)
    {
        mat.SetFloat("_Mode", 0);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        mat.SetInt("_ZWrite", 1);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = -1;
    }

    void OnDrawGizmosSelected()
    {
        if (mainCamera)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(mainCamera.transform.position, fadeDistance);
        }
    }
}
