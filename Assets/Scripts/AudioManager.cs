using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("FMOD Events")]
    [SerializeField] public EventReference LeverPullSFX;
    [SerializeField] public EventReference SwitchToMouseSFX;
    [SerializeField] public EventReference SwitchToMechSFX;
    [SerializeField] public EventReference MouseHurtSFX;
    [SerializeField] public EventReference MechHurtSFX;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public EventInstance PlaySFX(EventReference audioFile, Vector3? position = null, float volume = 1f)
    {
        if (audioFile.IsNull) return default;

        EventInstance instance = RuntimeManager.CreateInstance(audioFile);
        instance.setVolume(volume);

        if (position.HasValue)
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(position.Value));

        instance.start();
        return instance;
    }

    public EventInstance PlaySFXWithParams(
        EventReference audioFile,
        Dictionary<string, float> parameters,
        Vector3? position = null,
        float volume = 1f
    )
    {
        if (audioFile.IsNull) return default;

        EventInstance instance = RuntimeManager.CreateInstance(audioFile);
        instance.setVolume(volume);

        if (position.HasValue)
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(position.Value));

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                instance.setParameterByName(param.Key, param.Value);
            }
        }

        instance.start();
        return instance;
    }

    public void SetParameter(EventInstance instance, string paramName, float value)
    {
        if (!instance.isValid()) return;

        instance.setParameterByName(paramName, value);
    }

    public void StopSFX(EventInstance instance)
    {
        if (!instance.isValid()) return;

        instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        instance.release();
    }
}
