using UnityEngine;
using FMODUnity;

public class SpatialSoundPlayer : MonoBehaviour
{
    [Header("FMOD Settings")]
    public EventReference fmodEvent;
    [Range(0f, 2f)]
    public float volume = 1f;

    private FMOD.Studio.EventInstance currentInstance;

    private void Start()
    {
        PlaySound();
    }

    public void PlaySound()
    {
        if (fmodEvent.IsNull) return;
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioManager instance not found!");
            return;
        }

        currentInstance = AudioManager.Instance.PlaySFX(fmodEvent, transform.position, volume);
    }

    public void StopSound()
    {
        if (currentInstance.isValid())
        {
            AudioManager.Instance.StopSFX(currentInstance);
        }
    }

    private void OnDestroy()
    {
        StopSound();
    }
}