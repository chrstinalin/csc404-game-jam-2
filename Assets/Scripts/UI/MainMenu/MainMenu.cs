using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using FMODUnity;
using FMOD.Studio;

public class MainMenu : MonoBehaviour
{
    [System.Serializable]
    public class MenuState
    {
        public string stateName;
        public GameObject panel;
        public Selectable[] selectables;
        public Selectable defaultSelectable;
    }

    [Header("Menu States")]
    public MenuState[] menuStates;
    public MenuState currentState;
    private int currentSelection = 0;
    private float inputCooldown = 0f;
    private float repeatDelay = 0.2f;
    private float inputDeadzone = 0.5f;
    private bool axisInUse = false;
    private GameObject lastSelected;

    [Header("Audio")]
    [SerializeField] private EventReference MenuBGM;
    [SerializeField] private EventReference SelectSFX;
    [SerializeField] private EventReference UpSFX;
    [SerializeField] private EventReference DownSFX;
    [SerializeField] private EventReference BackSFX;

    private EventInstance musicInstance;

    void Start()
    {
        SwitchToState("MainMenu");
        musicInstance = AudioManager.Instance.PlaySFX(MenuBGM);

        if (BackgroundMusicManager.Instance != null)
        {
            BackgroundMusicManager.Instance.StopTheme();
            Destroy(BackgroundMusicManager.Instance.gameObject);
        }
            
    }

    void Update()
    {
        HandleControllerInput();
        PreventMouseDeselection();
    }

    void PreventMouseDeselection()
    {
        if (EventSystem.current.currentSelectedGameObject == null) 
            EventSystem.current.SetSelectedGameObject(lastSelected);
        else
            lastSelected = EventSystem.current.currentSelectedGameObject;
    }

    void HandleControllerInput()
    {
        if (inputCooldown > 0f)
            inputCooldown -= Time.deltaTime;

        if (Input.GetButtonDown("Cancel") && currentState.stateName != "MainMenu")
        {
            AudioManager.Instance.PlaySFX(BackSFX);
            GoBack();
            return;
        }

        float vertical = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(vertical) < inputDeadzone)
        {
            // Stick/keys returned to neutral: ready to register the next press right away.
            axisInUse = false;
        }
        else if (!axisInUse || inputCooldown <= 0f)
        {
            // Fire once on the initial press (!axisInUse), then auto-repeat while held
            // once the cooldown elapses. A single branch per frame means one move per
            // step — no double navigation from the axis flickering across thresholds.
            if (vertical < 0f)
                NavigateDown();
            else
                NavigateUp();

            axisInUse = true;
            inputCooldown = repeatDelay;
        }

        if (Input.GetButtonDown("Submit"))
        {
            AudioManager.Instance.PlaySFX(SelectSFX);
            if (EventSystem.current.currentSelectedGameObject != null)
            {
                Button selectedButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
                Toggle selectedToggle = EventSystem.current.currentSelectedGameObject.GetComponent<Toggle>();
                if (selectedButton != null)
                {
                    selectedButton.onClick.Invoke();
                }
                else if (selectedToggle != null)
                {
                    selectedToggle.isOn = !selectedToggle.isOn;
                }
            }
        }
    }

    void NavigateDown()
    {
        if (currentState.selectables.Length == 0) return;

        currentSelection++;
        if (currentSelection >= currentState.selectables.Length)
            currentSelection = 0;

        SelectItem(currentSelection);
        AudioManager.Instance.PlaySFX(DownSFX);
    }

    void NavigateUp()
    {
        if (currentState.selectables.Length == 0) return;

        currentSelection--;
        if (currentSelection < 0)
            currentSelection = currentState.selectables.Length - 1;

        SelectItem(currentSelection);
        AudioManager.Instance.PlaySFX(UpSFX);
    }

    void SelectItem(int index)
    {
        if (currentState.selectables.Length == 0) return;

        EventSystem.current.SetSelectedGameObject(currentState.selectables[index].gameObject);
        lastSelected = currentState.selectables[index].gameObject;
    }
    
    public void SwitchToState(string stateName)
    {
        if (currentState != null && currentState.panel != null)
            currentState.panel.SetActive(false);

        foreach (var state in menuStates)
        {
            if (state.stateName == stateName)
            {
                currentState = state;
                currentState.panel.SetActive(true);
                currentSelection = 0;

                if (currentState.defaultSelectable != null)
                {
                    EventSystem.current.SetSelectedGameObject(currentState.defaultSelectable.gameObject);
                    lastSelected = currentState.defaultSelectable.gameObject;
                }
                else if (currentState.selectables.Length > 0)
                {
                    SelectItem(0);
                }

                return;
            }
        }
    }

    void GoBack()
    {
        SwitchToState("MainMenu");
    }

    public void PlayGame()
    {
        StopMusic();
        FadeManager.Instance.FadeToScene("Level1Tutorial");
    }

    public void OpenControls()
    {
        SwitchToState("Controls");
    }

    public void OpenSettings()
    {
        SwitchToState("Settings");
    }

    public void OpenCredits()
    {
        SwitchToState("Credits");
    }

    public void PlayTutorial()
    {
        FadeManager.Instance.FadeToScene("TutorialLevel");
        StopMusic();
    }

    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
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