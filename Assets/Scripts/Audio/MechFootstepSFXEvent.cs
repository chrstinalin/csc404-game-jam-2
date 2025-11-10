using UnityEngine;
using FMODUnity;

public class MechFootstepSFXEvent : MonoBehaviour
{
    public EventReference MechStepTechSFX;
    public EventReference MechStepImpactSFX;

    public void MechStepTech()
    {
        RuntimeManager.PlayOneShot(MechStepTechSFX, transform.position);
     
    }
    public void MechStepImpact()
    {
        RuntimeManager.PlayOneShot(MechStepImpactSFX, transform.position);

    }
}
