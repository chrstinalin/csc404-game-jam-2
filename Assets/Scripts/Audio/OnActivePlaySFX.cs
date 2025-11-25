using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class OnActivePlaySFX : MonoBehaviour
{
    [SerializeField] private EventReference sfx;
    [SerializeField] private float volume = 1f;

    private EventInstance instance;

    private void OnEnable()
    {
        if (sfx.IsNull) return;

        instance = RuntimeManager.CreateInstance(sfx);
        instance.setVolume(volume);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        instance.start();
    }

    private void OnDisable()
    {
        if (instance.isValid())
        {
            instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            instance.release();
            instance.clearHandle();
        }
    }
}
