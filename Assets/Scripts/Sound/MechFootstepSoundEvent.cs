using UnityEngine;
using FMODUnity;

public class MechFootstepSoundEvent : MonoBehaviour
{
    public EventReference MechStepMechanicsSFX;
    public EventReference MechStepImpactSFX;
    public void MechStepMechanics()
    {
        RuntimeManager.PlayOneShot(MechStepMechanicsSFX, transform.position);
    }
    public void MechStepImpact()
    {
        RuntimeManager.PlayOneShot(MechStepImpactSFX, transform.position);
    }
}
