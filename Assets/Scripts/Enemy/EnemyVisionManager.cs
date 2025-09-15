using UnityEngine;

public class EnemyVisionManager : EnemyVisionAbstractManager
{
    private Color ConeColor = Color.white;
    private int coneSegments = 30;

    private float MouseDetectRange = 8;
    private float MechDetectRange = 20;

    private float MouseDetectAngle = 20;
    private float MechDetectAngle = 40;

    private void Start()
    {
        Mouse = GameObject.FindWithTag("MousePlayerEntity");
        Mech = GameObject.FindWithTag("MechPlayerEntity");
    }

    public override void InitVision()
    {
        InitConeRenderer(MechConeRenderer);
        InitConeRenderer(MouseConeRenderer);
    }
    public override void UpdateVision()
    {
        CheckPlayerInRange();
        DrawVisionCone(MouseConeRenderer, MouseDetectAngle, MouseDetectRange);
        DrawVisionCone(MechConeRenderer, MechDetectAngle, MechDetectRange);
    }

    private void InitConeRenderer(LineRenderer renderer)
    {
        renderer.positionCount = coneSegments + 2;
        renderer.widthMultiplier = 0.05f;
        renderer.startColor = ConeColor;
        renderer.endColor = ConeColor;
        renderer.loop = true;
    }
    public void CheckPlayerInRange()
    {
        RaycastHit MouseHit, MechHit;

        if (Physics.Raycast(transform.position, (Mouse.transform.position - transform.position), out MouseHit, Mathf.Infinity))
        {
            MouseIsHidden = MouseHit.transform != Mouse.transform;
        }
        if (Physics.Raycast(transform.position, (Mech.transform.position - transform.position), out MechHit, Mathf.Infinity))
        {
            MechIsHidden = MechHit.transform != Mech.transform;
        }

        Vector3 MouseDirectionVector = Mouse.transform.position - transform.position;
        Vector3 MechDirectionVector = Mech.transform.position - transform.position;
        float MouseAngle = Vector3.SignedAngle(MouseDirectionVector, transform.forward, Vector3.up);
        float MechAngle = Vector3.SignedAngle(MechDirectionVector, transform.forward, Vector3.up);

        MouseInAngle = (MouseAngle > -MouseDetectAngle && MouseAngle < MouseDetectAngle);
        MechInAngle = (MechAngle > -MechDetectAngle && MechAngle < MechDetectAngle);

        MouseInRange = Vector3.Distance(transform.position, Mouse.transform.position) < MouseDetectRange;
        MechInRange = Vector3.Distance(transform.position, Mech.transform.position) < MechDetectRange;

        MouseIsSpotted = MouseInRange && MouseInAngle && !MouseIsHidden;
        MechIsSpotted = MechInRange && MechInAngle && !MechIsHidden;
    }

    void DrawVisionCone(LineRenderer renderer, float angle, float range)
    {
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
}
