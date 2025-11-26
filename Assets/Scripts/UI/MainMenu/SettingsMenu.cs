using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private GameObject musicSettings;
    [SerializeField] private GameObject sfxSettings;
    [SerializeField] private int sliderSteps = 5;  // No. of steps; represents fraction of 1

    private Text musicVal;
    private Text sfxVal;
    private Slider musicSlider;
    private Slider sfxSlider;

    [Header("Nav")]
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button backButton;
    private float inputCooldown = 0f;
    private float cooldownTime = 0.2f;

    [Header("Audio")]
    [SerializeField] private FMODUnity.EventReference SelectSFX;
    [SerializeField] private FMODUnity.EventReference UpSFX;
    [SerializeField] private FMODUnity.EventReference DownSFX;

    void Start()
    {
        // Values
        musicVal = musicSettings.transform.Find("MusicVal").GetComponent<Text>();
        if (musicVal != null)
            musicVal.text = Mathf.RoundToInt(AudioManager.Instance.GetMusicVolume() * 100).ToString() + "%";

        sfxVal = sfxSettings.transform.Find("SFXVal").GetComponent<Text>();
        if (sfxVal != null)
            sfxVal.text = Mathf.RoundToInt(AudioManager.Instance.GetSFXVolume() * 100).ToString() + "%";

        // Slider
        musicSlider = musicSettings.GetComponentInChildren<Slider>();
        if (musicSlider != null)
        {
            musicSlider.value = AudioManager.Instance.GetMusicVolume() * sliderSteps;
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        sfxSlider = sfxSettings.GetComponentInChildren<Slider>();
        if (sfxSlider != null)
        {
            sfxSlider.value = AudioManager.Instance.GetSFXVolume() * sliderSteps;
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }

    void Update()
    {
        if (inputCooldown > 0)
        {
            inputCooldown -= Time.deltaTime;
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(horizontal) > 0.5f)
        {
            AdjustCurrentSlider(horizontal);
        }
    }

    void AdjustCurrentSlider(float direction)
    {
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null) return;

        Slider slider = selected.GetComponent<Slider>();
        if (slider == null) return;

        float newValue = slider.value + (direction > 0 ? 1 : -1);
        newValue = Mathf.Clamp(newValue, slider.minValue, slider.maxValue);
        
        slider.value = newValue;
        inputCooldown = cooldownTime;
    }

    void OnMusicVolumeChanged(float value)
    {
        musicVal.text = Mathf.RoundToInt(value * (100 / sliderSteps)).ToString() + "%";
        AudioManager.Instance.SetMusicVolume(value / sliderSteps);
    }

    void OnSFXVolumeChanged(float value)
    {
        sfxVal.text = Mathf.RoundToInt(value * (100 / sliderSteps)).ToString() + "%";
        AudioManager.Instance.SetSFXVolume(value / sliderSteps);
    }

}