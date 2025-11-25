using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("FMOD Events")]
    [SerializeField] public EventReference ButtonPressSFX;
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
    public void PlaySFX(EventReference audioFile, Vector3? position = null, float volume = 1f)
    {
        if (audioFile.IsNull) return;

        EventInstance instance = RuntimeManager.CreateInstance(audioFile);

        // Set 3D position if provided
        if (position.HasValue)
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(position.Value));

        // Set volume
        instance.setVolume(volume);

        instance.start();
        instance.release();
    }
}
