using UnityEngine;

public abstract class EnemyVisionAbstractManager : MonoBehaviour
{
    public GameObject Mouse;
    public GameObject Mech;

    public LineRenderer MechConeRenderer;
    public LineRenderer MouseConeRenderer;

    [HideInInspector] public bool MouseIsSpotted = false;
    [HideInInspector] public bool MechIsSpotted = false;

    [HideInInspector] public bool MouseInRange = false;
    [HideInInspector] public bool MechInRange = false;

    [HideInInspector] public bool MouseIsHidden = false;
    [HideInInspector] public bool MechIsHidden = false;

    [HideInInspector] public bool MouseInAngle = false;
    [HideInInspector] public bool MechInAngle = false;

    public abstract void InitVision();
    public abstract void UpdateVision();
}
