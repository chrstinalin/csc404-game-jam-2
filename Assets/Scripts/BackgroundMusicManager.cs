using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;
using FMOD.Studio;

public class BackgroundMusicManager : MonoBehaviour
{
    public static BackgroundMusicManager Instance;

    [Header("FMOD Events")]
    [SerializeField] private EventReference MainThemeAudio;
    [SerializeField] private EventReference AmbienceAudio;

    private EventInstance mainThemeInstance;
    private EventInstance ambienceInstance;
    private bool isInCombat = false;

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
        PlayAmbience();
        SetCombatParameter(false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayTheme();
        PlayAmbience();           
        SetCombatParameter(false);
    }

    void Update()
    {
        if (!mainThemeInstance.isValid()) return;

        EnemyVisionManager[] allEnemies = FindObjectsOfType<EnemyVisionManager>();
        int activeEnemiesCount = 0;

        foreach (var enemy in allEnemies)
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                activeEnemiesCount++;
            }
        }

        Debug.Log($"[BackgroundMusicManager] Active enemies: {activeEnemiesCount}");

        bool enemiesActive = activeEnemiesCount > 0;

        if (enemiesActive != isInCombat)
        {
            SetCombatParameter(enemiesActive);
        }
    }

    private void SetCombatParameter(bool inCombat)
    {
        isInCombat = inCombat;
        float value = inCombat ? 0f : 1f;

        if (mainThemeInstance.isValid())
            mainThemeInstance.setParameterByName("Combat", value);
    }

    public void PauseMenu(bool isPaused)
    {
        if (mainThemeInstance.isValid())
            mainThemeInstance.setParameterByName("Pause", isPaused ? 1f : 0f);
    }

    public void LockOnMode(bool isLocked)
    {
        if (mainThemeInstance.isValid())
            mainThemeInstance.setParameterByName("Lock On", isLocked ? 1f : 0f);
    }

    public void ResetSettings()
    {
        if (mainThemeInstance.isValid())
        {
            mainThemeInstance.setParameterByName("Pause", 0f);
            mainThemeInstance.setParameterByName("Lock On", 0f);
        }
    }

    public void PlayTheme()
    {
        if (MainThemeAudio.IsNull) return;

        if (!mainThemeInstance.isValid())
        {
            mainThemeInstance = RuntimeManager.CreateInstance(MainThemeAudio);
            mainThemeInstance.start();
        }
    }

    private void PlayAmbience()
    {
        if (AmbienceAudio.IsNull) return;

        if (!ambienceInstance.isValid())
        {
            ambienceInstance = RuntimeManager.CreateInstance(AmbienceAudio);
            ambienceInstance.start();
        }
    }

    public void StopTheme()
    {
        if (mainThemeInstance.isValid())
        {
            mainThemeInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            mainThemeInstance.release();
        }

        if (ambienceInstance.isValid())
        {
            ambienceInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            ambienceInstance.release();
        }
    }

    public void SwitchTheme(EventReference newTheme)
    {
        if (newTheme.IsNull) return;

        EventInstance newThemeInstance = RuntimeManager.CreateInstance(newTheme);
        newThemeInstance.start();

        if (mainThemeInstance.isValid())
        {
            mainThemeInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            mainThemeInstance.release();
        }

        mainThemeInstance = newThemeInstance;
    }
}
