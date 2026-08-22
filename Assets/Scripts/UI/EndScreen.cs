using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using FMODUnity;
using FMOD.Studio;

public class EndScreen : MonoBehaviour
{
    public GameObject button;
    
    [Header("Audio")]
    [SerializeField] private EventReference EndScreenBGM;
    [SerializeField] private EventReference BackSFX;
    
    private EventInstance musicInstance;

    void Start()
    {
        FadeManager.Instance.FadeIn();
        
        if (BackgroundMusicManager.Instance != null) {
            BackgroundMusicManager.Instance.StopTheme();
            Destroy(BackgroundMusicManager.Instance.gameObject);
        }
        if (AudioManager.Instance != null) musicInstance = AudioManager.Instance.PlaySFX(EndScreenBGM);
        
        GameInput.TakeOverMenuSubmit();
        EventSystem.current.SetSelectedGameObject(button);
    }

    void Update()
    {
        if (GameInput.SubmitDown)
        {
            LoadMainMenu();
        }
    }
    
    public void LoadMainMenu()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(BackSFX);
        
        StopMusic();
        FadeManager.Instance.FadeToScene("MainMenu");
    }
    
    void StopMusic()
    {
        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();
        }
    }
    
    void OnDestroy()
    {
        StopMusic();
    }
}