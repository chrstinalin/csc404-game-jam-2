using UnityEngine;
using FMODUnity;

public class PeanutStepSFX : MonoBehaviour
{
    public EventReference PnStepSFX;
    public EventReference PnJumpSFX;

    public void PnStep()
    {
        RuntimeManager.PlayOneShot(PnStepSFX, transform.position);

    }
    public void PnJump()
    {
        RuntimeManager.PlayOneShot(PnJumpSFX, transform.position);

    }
}
