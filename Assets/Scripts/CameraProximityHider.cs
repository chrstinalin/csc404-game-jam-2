using System.Collections.Generic;
using UnityEngine;

public class CameraProximityHider : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float hideDistance = 4f;
    [SerializeField] private float showDistance = 5.0f;
    [Range(0.1f, 2f)]
    [SerializeField] private float fadeDuration = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float minAlpha = 0.1f;
    
    private Dictionary<Renderer, WallFader> wallFaders = new Dictionary<Renderer, WallFader>();
    private Camera cam;
    private GameObject[] allWalls;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;
        
        // Make sure showDistance is always greater than hideDistance
        if (showDistance <= hideDistance)
        {
            showDistance = hideDistance + 0.5f;
        }
        
        FindAllWalls();
    }
    
    void FindAllWalls()
    {
        allWalls = GameObject.FindGameObjectsWithTag("Wall");
        Debug.Log($"Found {allWalls.Length} walls in scene");
    }
    
    void LateUpdate()
    {
        if (cam == null) return;
        
        HashSet<Renderer> shouldBeHidden = new HashSet<Renderer>();
        
        // Check distance to all walls
        foreach (GameObject wall in allWalls)
        {
            if (wall == null) continue;
            
            // Skip the player
            if (PlayerMech.Instance != null && 
                (wall.transform == PlayerMech.Instance.transform || 
                 wall.transform.IsChildOf(PlayerMech.Instance.transform)))
                continue;
            
            Renderer renderer = wall.GetComponent<Renderer>();
            if (renderer == null) continue;
            
            // Use bounds to get closest point on the mesh to the camera
            float distance = Vector3.Distance(cam.transform.position, renderer.bounds.ClosestPoint(cam.transform.position));
            
            // Use hysteresis: once fading, check against showDistance instead of hideDistance
            bool isAlreadyFading = wallFaders.ContainsKey(renderer);
            float thresholdDistance = isAlreadyFading ? showDistance : hideDistance;
            
            if (distance < thresholdDistance)
            {
                shouldBeHidden.Add(renderer);
                
                // Create fader if it doesn't exist
                if (!isAlreadyFading)
                {
                    wallFaders[renderer] = new WallFader(renderer);
                }
            }
        }
        
        // Update all faders
        List<Renderer> fadersToRemove = new List<Renderer>();
        
        float fadeSpeed = 1f / fadeDuration;
        float deltaAlpha = fadeSpeed * Time.deltaTime;
        
        foreach (var kvp in wallFaders)
        {
            Renderer renderer = kvp.Key;
            WallFader fader = kvp.Value;
            
            if (renderer == null)
            {
                fadersToRemove.Add(renderer);
                continue;
            }
            
            bool shouldHide = shouldBeHidden.Contains(renderer);
            float targetAlpha = shouldHide ? minAlpha : 1f;
            
            fader.UpdateFade(targetAlpha, deltaAlpha);
            
            // Remove fader if it's fully visible and not being hidden
            if (!shouldHide && fader.IsFullyVisible())
            {
                fader.Cleanup();
                fadersToRemove.Add(renderer);
            }
        }
        
        // Clean up removed faders
        foreach (Renderer renderer in fadersToRemove)
        {
            wallFaders.Remove(renderer);
        }
    }
    
    void OnDestroy()
    {
        // Restore all materials on cleanup
        foreach (var fader in wallFaders.Values)
        {
            fader.Cleanup();
        }
        wallFaders.Clear();
    }
    
    private class WallFader
    {
        private Renderer renderer;
        private Material[] originalMaterials;
        private Material[] fadedMaterials;
        private float currentAlpha = 1f;
        private bool isSetup = false;
        
        public WallFader(Renderer renderer)
        {
            this.renderer = renderer;
            this.originalMaterials = renderer.sharedMaterials;
        }
        
        private void SetupFadedMaterials()
        {
            if (isSetup) return;
            
            fadedMaterials = new Material[originalMaterials.Length];
            for (int i = 0; i < originalMaterials.Length; i++)
            {
                fadedMaterials[i] = new Material(originalMaterials[i]);
                SetupTransparentMaterial(fadedMaterials[i]);
            }
            
            renderer.materials = fadedMaterials;
            isSetup = true;
        }
        
        private void SetupTransparentMaterial(Material mat)
        {
            // For URP/Lit shader, set Surface Type to Transparent
            if (mat.HasProperty("_Surface"))
            {
                mat.SetFloat("_Surface", 1);
            }
            
            // Set blend mode to Alpha
            if (mat.HasProperty("_Blend"))
            {
                mat.SetFloat("_Blend", 0);
            }
            
            // Enable alpha clipping if available
            if (mat.HasProperty("_AlphaClip"))
            {
                mat.SetFloat("_AlphaClip", 0);
            }
            
            // Set render queue for transparency
            mat.renderQueue = 3000;
            
            // Enable the correct keywords for URP transparency
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            
            // Set blend modes
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
        }
        
        public void UpdateFade(float targetAlpha, float deltaAlpha)
        {
            if (renderer == null) return;
            
            // Only setup materials if we need to fade
            if (targetAlpha < 1f && !isSetup)
            {
                SetupFadedMaterials();
            }
            
            if (!isSetup) return;
            
            // Smoothly transition alpha
            currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, deltaAlpha);
            
            // Apply alpha to all materials
            foreach (Material mat in fadedMaterials)
            {
                if (mat != null)
                {
                    // URP uses _BaseColor instead of _Color
                    if (mat.HasProperty("_BaseColor"))
                    {
                        Color color = mat.GetColor("_BaseColor");
                        color.a = currentAlpha;
                        mat.SetColor("_BaseColor", color);
                    }
                    // Fallback to _Color for other shaders
                    else if (mat.HasProperty("_Color"))
                    {
                        Color color = mat.color;
                        color.a = currentAlpha;
                        mat.color = color;
                    }
                }
            }
        }
        
        public bool IsFullyVisible()
        {
            return currentAlpha >= 0.99f;
        }
        
        public void Cleanup()
        {
            if (renderer != null && isSetup)
            {
                renderer.sharedMaterials = originalMaterials;
            }
            
            if (fadedMaterials != null)
            {
                foreach (Material mat in fadedMaterials)
                {
                    if (mat != null)
                    {
                        Object.Destroy(mat);
                    }
                }
            }
        }
    }
}