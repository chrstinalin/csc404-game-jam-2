using UnityEngine;

public class EnemyVisionManager : EnemyVisionAbstractManager
{
    private Color ConeColor = Color.white;
    private int coneSegments = 30;

    private float MouseDetectRange = 8;
    private float MechDetectRange = 12;

    private float MouseDetectAngle = 20;
    private float MechDetectAngle = 40;

    private float mouseFadeStartDistance = 30f;
    private float mechFadeStartDistance = 50f;
    private float fadeSpeed = 3f;
    private float mouseAlpha = 0f;
    private float mechAlpha = 0f;

    private MovementManager movementManager;

    private void Start()
    {   
        if (Mouse == null)
            Mouse = GameObject.FindWithTag("MousePlayerEntity");
        if (Mech == null)
            Mech = GameObject.FindWithTag("MechPlayerEntity");
            
        movementManager = MovementManager.Instance;
    }

    public override void InitVision()
    {
        InitConeRenderer(MechConeRenderer);
        InitConeRenderer(MouseConeRenderer);
    }
    
    public override void UpdateVision()
    {
        CheckPlayerInRange();
        
        float mouseDistance = Vector3.Distance(transform.position, Mouse.transform.position);
        float mechDistance = Vector3.Distance(transform.position, Mech.transform.position);
        
        bool showMouseCone = false;
        bool showMechCone = false;
        
        if (movementManager != null && movementManager.IsMouseActive && MouseInRange)
        {
            showMouseCone = true;
        }
        
        if (MechInRange)
        {
            showMechCone = true;
        }
        
        float targetMouseAlpha = showMouseCone ? CalculateFadeAlpha(mouseDistance, MouseDetectRange, 
            mouseFadeStartDistance) : 0f;
        float targetMechAlpha = showMechCone ? CalculateFadeAlpha(mechDistance, MechDetectRange, 
            mechFadeStartDistance) : 0f;
        
        mouseAlpha = Mathf.Lerp(mouseAlpha, targetMouseAlpha, Time.deltaTime * fadeSpeed);
        mechAlpha = Mathf.Lerp(mechAlpha, targetMechAlpha, Time.deltaTime * fadeSpeed);
        
        if (mouseAlpha > 0.01f && OnSameVerticalPlane(Mouse))
        {
            UpdateConeAlpha(MouseConeRenderer, mouseAlpha);
            DrawVisionCone(MouseConeRenderer, MouseDetectAngle, MouseDetectRange);
        }
        else
        {
            MouseConeRenderer.enabled = false;
        }
        
        if (mechAlpha > 0.01f && OnSameVerticalPlane(Mech))
        {
            UpdateConeAlpha(MechConeRenderer, mechAlpha);
            DrawVisionCone(MechConeRenderer, MechDetectAngle, MechDetectRange);
        }
        else
        {
            MechConeRenderer.enabled = false;
        }
    }

    private void InitConeRenderer(LineRenderer renderer)
    {
        renderer.positionCount = coneSegments + 2;
        renderer.widthMultiplier = 0.05f;
        renderer.startColor = ConeColor;
        renderer.endColor = ConeColor;
        renderer.loop = true;
    }
    
    private float CalculateFadeAlpha(float distance, float maxRange, float fadeStartDist)
    {
        if (distance >= fadeStartDist) return 0f;
        else if (distance <= maxRange * 0.5f) return 1f;
        else return 1f - Mathf.InverseLerp(maxRange * 0.5f, fadeStartDist, distance);
    }
    
    private void UpdateConeAlpha(LineRenderer renderer, float alpha)
    {
        Color startColor = ConeColor;
        Color endColor = ConeColor;
        startColor.a = alpha;
        endColor.a = alpha;
        renderer.startColor = startColor;
        renderer.endColor = endColor;
    }
    
    public void CheckPlayerInRange()
    {
        if (Mouse == null || Mech == null)
        {
            Debug.LogWarning("Mouse or Mech reference is null in EnemyVisionManager!");
            return;
        }
        
        int layerMask = ~LayerMask.GetMask("Ignore Raycast");
        
        RaycastHit MouseHit, MechHit;

        if (Physics.Raycast(transform.position, (Mouse.transform.position - transform.position), 
                out MouseHit, Mathf.Infinity, layerMask))
        {
            MouseIsHidden = MouseHit.transform != Mouse.transform && !MouseHit.transform.IsChildOf(Mouse.transform);
        }
        
        if (Physics.Raycast(transform.position, (Mech.transform.position - transform.position), 
                out MechHit, Mathf.Infinity, layerMask))
        {
            MechIsHidden = MechHit.transform != Mech.transform && !MechHit.transform.IsChildOf(Mech.transform);
        }

        Vector3 MouseDirectionVector = Mouse.transform.position - transform.position;
        Vector3 MechDirectionVector = Mech.transform.position - transform.position;
        float MouseAngle = Vector3.SignedAngle(MouseDirectionVector, transform.forward, Vector3.up);
        float MechAngle = Vector3.SignedAngle(MechDirectionVector, transform.forward, Vector3.up);

        MouseInAngle = (MouseAngle > -MouseDetectAngle && MouseAngle < MouseDetectAngle);
        MechInAngle = (MechAngle > -MechDetectAngle && MechAngle < MechDetectAngle);

        float mouseDistance = Vector3.Distance(transform.position, Mouse.transform.position);
        float mechDistance = Vector3.Distance(transform.position, Mech.transform.position);
        
        MouseInRange = mouseDistance < MouseDetectRange;
        MechInRange = mechDistance < MechDetectRange;

        MouseIsSpotted = MouseInRange && MouseInAngle && !MouseIsHidden;
        MechIsSpotted = MechInRange && MechInAngle && !MechIsHidden;
    }

    void DrawVisionCone(LineRenderer renderer, float angle, float range)
    {
        renderer.enabled = true;
        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;
        renderer.SetPosition(0, origin);

        float halfAngle = angle;
        float angleStep = (halfAngle * 2f) / coneSegments;

        for (int i = 0; i <= coneSegments; i++)
        {
            float currentAngle = -halfAngle + i * angleStep;
            Vector3 direction = Quaternion.AngleAxis(currentAngle, Vector3.up) * forward;
            renderer.SetPosition(i + 1, origin + direction * range);
        }
    }

    bool OnSameVerticalPlane(GameObject target)
    {
        return Mathf.Abs(target.transform.position.z - transform.position.z) < 5f;
    }

}