using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Manages the current overworld theme.
// Manages an AudioSource to play sound effects.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [HideInInspector] public AudioSource currentlyPlaying;
    [HideInInspector] public AudioSource MainTheme;

    [SerializeField] private AudioClip MainThemeAudio;
    [SerializeField] public AudioClip MouseFootSteps;
    [SerializeField] public AudioClip MechFootSteps;
    [SerializeField] public AudioClip ButtonPressSFX;

    private float FADE_IN_TRANSITION = 2f;

    // Footstep loop state
    private Coroutine footstepCoroutine;
    private GameObject footstepSFXObj;
    private AudioSource footstepSource;
    private AudioClip currentFootstepClip;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if(!MainThemeAudio) Debug.LogWarning("Main Theme Audio not assigned in AudioManager.");

        this.MainTheme = gameObject.AddComponent<AudioSource>();
        this.MainTheme.clip = MainThemeAudio;
        this.MainTheme.loop = true;
        this.MainTheme.Play();

        currentlyPlaying = this.MainTheme;
    }

    public void PlaySFX(AudioClip audioFile)
    {
        if(!audioFile)
        {
            Debug.LogWarning("No audio file was provided to PlaySFX().");
            return;
        }

        GameObject SFX = new GameObject("SFX");
        AudioSource audioSource = SFX.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = audioFile;
        audioSource.Play();
        Destroy(SFX, audioFile.length);
    }

    public void HandleFootstepLoop(AudioClip clip, bool isMoving)
    {
        if (isMoving) StartFootstepLoop(clip);
        else StopFootstepLoop();
    }

    public void StartFootstepLoop(AudioClip clip)
    {
        if (!clip)
        {
            Debug.LogWarning("No audio clip was provided to StartFootstepLoop().");
            return;
        }

        if (footstepCoroutine != null && currentFootstepClip == clip) return;

        StopFootstepLoop();

        footstepSFXObj = new GameObject("FootstepSFX");
        footstepSource = footstepSFXObj.AddComponent<AudioSource>();
        footstepSource.playOnAwake = false;
        footstepSource.loop = false;

        currentFootstepClip = clip;
        footstepCoroutine = StartCoroutine(FootstepLoopCoroutine());
    }

    public void StopFootstepLoop()
    {
        if (footstepCoroutine != null)
        {
            StopCoroutine(footstepCoroutine);
            footstepCoroutine = null;
        }

        if (footstepSFXObj != null)
        {
            Destroy(footstepSFXObj);
            footstepSFXObj = null;
            footstepSource = null;
            currentFootstepClip = null;
        }
    }

    private IEnumerator FootstepLoopCoroutine()
    {
        while (true)
        {
            if (currentFootstepClip == null)  yield break;
            footstepSource.PlayOneShot(currentFootstepClip);
            float wait = currentFootstepClip.length + Config.FOOTSTEP_INTERVAL;
            yield return new WaitForSeconds(wait);
        }
    }

    public void SwitchTheme(AudioSource newTheme)
    {
        if(newTheme == currentlyPlaying) return;

        newTheme.time = currentlyPlaying.time;
        newTheme.volume = 0f;
        newTheme.Play();
        
        StartCoroutine(CrossfadeTheme(currentlyPlaying, newTheme));
        currentlyPlaying = newTheme;
    }

    private IEnumerator CrossfadeTheme(AudioSource from, AudioSource to)
    {
        float timer = 0f;
        while (timer < FADE_IN_TRANSITION)
        {
            float t = timer / FADE_IN_TRANSITION;
            from.volume = Mathf.Lerp(1f, 0f, t);
            to.volume = Mathf.Lerp(0f, 1f, t);
            timer += Time.deltaTime;
            yield return null;
        }
        from.volume = 0f;
        to.volume = 1f;
        from.Stop();
    }
}
