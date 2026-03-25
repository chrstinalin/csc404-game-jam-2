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
        
        EventSystem.current.SetSelectedGameObject(button);
    }

    void Update()
    {
        if (Input.GetButtonDown("Submit"))
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