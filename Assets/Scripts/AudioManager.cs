using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("FMOD Events")]
    [SerializeField] public EventReference LeverPullSFX;

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

    public void StopSFX(EventInstance instance)
    {
        if (!instance.isValid()) return;

        instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        instance.release();
    }
}
