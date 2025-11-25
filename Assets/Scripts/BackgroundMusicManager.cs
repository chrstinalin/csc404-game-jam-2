using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;

public class BackgroundMusicManager : MonoBehaviour
{
    public static BackgroundMusicManager Instance;

    [Header("FMOD Event")]
    [SerializeField] private EventReference MainThemeAudio;

    private FMOD.Studio.EventInstance mainThemeInstance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicates
            return;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        PlayTheme();
    }

    private void OnSceneLoaded(Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        PlayTheme();
    }

    public void PlayTheme()
    {
        if (MainThemeAudio.IsNull) return;

        if (!mainThemeInstance.isValid())
        {
            mainThemeInstance = FMODUnity.RuntimeManager.CreateInstance(MainThemeAudio);
            mainThemeInstance.start();
        }
    }

    public void StopTheme()
    {
        if (mainThemeInstance.isValid())
        {
            mainThemeInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            mainThemeInstance.release();
        }
    }

    public void SwitchTheme(FMODUnity.EventReference newTheme)
    {
        if (newTheme.IsNull) return;

        FMOD.Studio.EventInstance newThemeInstance = FMODUnity.RuntimeManager.CreateInstance(newTheme);
        newThemeInstance.start();

        if (mainThemeInstance.isValid())
        {
            mainThemeInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            mainThemeInstance.release();
        }

        mainThemeInstance = newThemeInstance;
    }
}
