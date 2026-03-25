using UnityEngine;
using FMODUnity;

public class MechFootstepSFX : MonoBehaviour
{
    public EventReference MechStepTechSFX;
    public EventReference MechStepImpactSFX;

    public void MechStepTech()
    {
        RuntimeManager.PlayOneShotAttached(MechStepTechSFX, gameObject);
    }
    
    public void MechStepImpact()
    {
        RuntimeManager.PlayOneShotAttached(MechStepImpactSFX, gameObject);
    }
}
