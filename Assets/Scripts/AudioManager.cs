using UnityEngine;
using System.Collections;
using FMODUnity;
using FMOD.Studio;

// Manages the current overworld theme.
// Manages FMOD events to play sound effects.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [HideInInspector] public EventInstance currentlyPlaying;
    [HideInInspector] public EventInstance MainTheme;

    [SerializeField] public EventReference MainThemeAudio;
    [SerializeField] public EventReference MouseFootSteps;
    [SerializeField] public EventReference MechFootSteps;
    [SerializeField] public EventReference ButtonPressSFX;
    [SerializeField] public EventReference BoxMoveSFX;

    private float FADE_IN_TRANSITION = 2f;

    // Footstep loop state
    private Coroutine footstepCoroutine;
    private EventInstance footstepInstance;
    private EventReference currentFootstepEvent;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (MainThemeAudio.IsNull)
        {
            Debug.LogWarning("Main Theme Audio not assigned in AudioManager.");
            return;
        }

        MainTheme = RuntimeManager.CreateInstance(MainThemeAudio);
        MainTheme.start();
        MainTheme.setVolume(1f);

        currentlyPlaying = MainTheme;
    }

    public void PlaySFX(EventReference audioFile)
    {
        if (audioFile.IsNull)
        {
            Debug.LogWarning("No FMOD event was provided to PlaySFX().");
            return;
        }

        RuntimeManager.PlayOneShot(audioFile, transform.position);
    }

    public void HandleFootstepLoop(EventReference clip, bool isMoving)
    {
        if (isMoving) StartFootstepLoop(clip);
        else StopFootstepLoop();
    }

    public void StartFootstepLoop(EventReference clip)
    {
        if (clip.IsNull)
        {
            Debug.LogWarning("No FMOD event was provided to StartFootstepLoop().");
            return;
        }

        if (footstepCoroutine != null && currentFootstepEvent.Guid == clip.Guid)
            return;

        StopFootstepLoop();

        currentFootstepEvent = clip;
        footstepCoroutine = StartCoroutine(FootstepLoopCoroutine());
    }

    public void StopFootstepLoop()
    {
        if (footstepCoroutine != null)
        {
            StopCoroutine(footstepCoroutine);
            footstepCoroutine = null;
        }

        if (footstepInstance.isValid())
        {
            footstepInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            footstepInstance.release();
        }

        currentFootstepEvent = new EventReference();
    }

    private IEnumerator FootstepLoopCoroutine()
    {
        while (true)
        {
            if (currentFootstepEvent.IsNull)
                yield break;

            RuntimeManager.PlayOneShot(currentFootstepEvent, transform.position);

            // Replace Config.FOOTSTEP_INTERVAL with your own interval constant or variable
            float wait = Config.FOOTSTEP_INTERVAL;
            yield return new WaitForSeconds(wait);
        }
    }

    public void SwitchTheme(EventReference newThemeAudio)
    {
        if (newThemeAudio.IsNull)
            return;

        if (currentlyPlaying.isValid())
        {
            StartCoroutine(CrossfadeTheme(currentlyPlaying, newThemeAudio));
        }
        else
        {
            MainTheme = RuntimeManager.CreateInstance(newThemeAudio);
            MainTheme.start();
            currentlyPlaying = MainTheme;
        }
    }

    private IEnumerator CrossfadeTheme(EventInstance from, EventReference toEvent)
    {
        EventInstance to = RuntimeManager.CreateInstance(toEvent);
        to.start();
        to.setVolume(0f);

        float timer = 0f;
        while (timer < FADE_IN_TRANSITION)
        {
            float t = timer / FADE_IN_TRANSITION;
            from.setVolume(Mathf.Lerp(1f, 0f, t));
            to.setVolume(Mathf.Lerp(0f, 1f, t));
            timer += Time.deltaTime;
            yield return null;
        }

        from.setVolume(0f);
        to.setVolume(1f);
        from.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        from.release();
        currentlyPlaying = to;
        MainTheme = to;
    }

    void OnDestroy()
    {
        if (MainTheme.isValid())
        {
            MainTheme.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            MainTheme.release();
        }
    }
}