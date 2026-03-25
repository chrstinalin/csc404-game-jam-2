using UnityEngine;
using FMODUnity;

public class PeanutStepSFX : MonoBehaviour
{
    public EventReference PnStepSFX;
    public EventReference PnJumpSFX;

    public void PnStep()
    {
        RuntimeManager.PlayOneShotAttached(PnStepSFX, gameObject);
    }
    
    public void PnJump()
    {
        RuntimeManager.PlayOneShotAttached(PnJumpSFX, gameObject);
    }
}
