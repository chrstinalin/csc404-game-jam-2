using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using FMODUnity;

public class UIManager : MonoBehaviour
{
    // Mouse Health UI
    [NonSerialized] private Health MouseHealth;
    [NonSerialized] private Image[] HealthPoints;

    // Mech Health UI
    [NonSerialized] private Health MechHealth;
    [NonSerialized] private Image HealthFront;
    [NonSerialized] private Image HealthBack;
    [NonSerialized] private float lerpTimer;
    [NonSerialized] private float DAMAGE_LERP = 5f;
    [NonSerialized] private float CHIP_LERP = 50f;
    
    // Pause Menu
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private Volume volume;
    private DepthOfField dof;
    public static bool GamePaused = false;
    
    // Pause Menu Navigation
    [SerializeField] private Selectable[] pauseSelectables;
    [SerializeField] private Selectable defaultPauseSelectable;
    [SerializeField] private Selectable[] controlsSelectables;
    [SerializeField] private Selectable defaultControlsSelectable;
    
    private int currentSelection = 0;
    private float inputCooldown = 0f;
    private float cooldownTime = 0.2f;
    private GameObject lastSelected;
    
    // Audio
    [SerializeField] private EventReference SelectSFX;
    [SerializeField] private EventReference UpSFX;
    [SerializeField] private EventReference DownSFX;
    [SerializeField] private EventReference BackSFX;

    private void Start()
    {
        InitializeHealthUI();
        InitializePauseMenu();
    }
    
    private void InitializeHealthUI()
    {
        GameObject HealthPointContainer = GameObject.FindGameObjectWithTag("MouseHealthPointContainer");
        GameObject _HealthFront = GameObject.FindGameObjectWithTag("MechHealthFront");
        GameObject _HealthBack = GameObject.FindGameObjectWithTag("MechHealthBack");

        if(!HealthPointContainer || !_HealthFront || !_HealthBack)
        {
            Debug.LogError("UI elements not found.");
            return;
        }

        // MOUSE
        HealthPoints = HealthPointContainer.GetComponentsInChildren<Image>();

        if (PlayerMouse.Instance != null)
        {
            MouseHealth = PlayerMouse.Instance.GetComponent<Health>();
            if (MouseHealth != null)
            {
                MouseHealth.onHealthChanged.AddListener(OnMouseHealthChanged);
            }
        }

        // MECH
        HealthFront = _HealthFront.GetComponent<Image>();
        HealthBack = _HealthBack.GetComponent<Image>();

        if (PlayerMech.Instance != null)
        {
            MechHealth = PlayerMech.Instance.GetComponent<Health>();
            if (MechHealth != null)
            {
                MechHealth.onMaxedHealth.AddListener(OnMechMaxedHealth);
                MechHealth.onHealthChanged.AddListener(OnMechHealthChanged);
            }
        }
    }

    private void InitializePauseMenu()
    {
        if (volume != null && volume.profile.TryGet(out dof)) dof.active = false;
        
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        
        GamePaused = false;
    }
    
    private void Update()
    {
        updateMechHealthUI();
        HandlePauseInput();
        
        if (GamePaused)
        {
            HandlePauseMenuNavigation();
        }
    }

    private void updateMechHealthUI()
    {
        if(!HealthFront || !HealthBack || !MechHealth) return;

        float fillA = HealthFront.fillAmount;
        float fillB = HealthBack.fillAmount;
        float hFraction = (float) MechHealth.GetCurrHealth() / MechHealth.GetMaxHealth();
        if (fillB > hFraction)
        {
            HealthBack.color = Color.red;
            HealthFront.fillAmount = hFraction;

            lerpTimer += Time.deltaTime;

            HealthFront.fillAmount = Mathf.Lerp(fillA, hFraction, lerpTimer / DAMAGE_LERP);
            HealthBack.fillAmount = Mathf.Lerp(fillB, hFraction, lerpTimer / CHIP_LERP);
        }
        else if (fillA < hFraction)
        {
            HealthBack.color = Color.green;
            HealthBack.fillAmount = hFraction;
            lerpTimer += Time.deltaTime;

            HealthFront.fillAmount = Mathf.Lerp(fillA, hFraction, lerpTimer / DAMAGE_LERP);
            HealthBack.fillAmount = Mathf.Lerp(fillB, hFraction, lerpTimer / CHIP_LERP);
        }
    }

    public void OnMechHealthChanged(int damage) => lerpTimer = 0;

    private void OnMechMaxedHealth(int HealthChange)
    {
        if (HealthFront != null) HealthFront.fillAmount = 1f;
        if (HealthBack != null) HealthBack.fillAmount = 1f;
    }

    public void OnMouseHealthChanged(int damage)
    {
        if (MouseHealth == null || HealthPoints == null) return;
        
        for (int i = 0; i < MouseHealth.GetMaxHealth(); i++)
        {
            if (i < HealthPoints.Length)
            {
                HealthPoints[i].enabled = i <= MouseHealth.GetCurrHealth() - 1;
            }
        }
    }
    
    private void HandlePauseInput()
    {
        if (Input.GetButtonDown("Pause"))
        {
            if (GamePaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }
    
    private void HandlePauseMenuNavigation()
    {
        if (inputCooldown > 0)
        {
            inputCooldown -= Time.unscaledDeltaTime;
            return;
        }

        if (Input.GetButtonDown("Cancel"))
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(BackSFX);
                
            if (controlsPanel != null && controlsPanel.activeSelf)
            {
                CloseControls();
            }
            else
            {
                Resume();
            }
            inputCooldown = cooldownTime;
            return;
        }

        float vertical = Input.GetAxisRaw("Vertical");

        if (vertical < -0.5f)
            NavigateDown();
        else if (vertical > 0.5f)
            NavigateUp();

        if (Input.GetButtonDown("Submit"))
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(SelectSFX);
                
            if (EventSystem.current.currentSelectedGameObject != null)
            {
                Button selectedButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
                if (selectedButton != null) selectedButton.onClick.Invoke();
            }
        }
    }

    void NavigateDown()
    {
        Selectable[] currentSelectables = controlsPanel.activeSelf ? controlsSelectables : pauseSelectables;
        
        if (currentSelectables.Length == 0) return;

        currentSelection++;
        if (currentSelection >= currentSelectables.Length)
            currentSelection = 0;

        SelectItem(currentSelection, currentSelectables);
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(DownSFX);
            
        inputCooldown = cooldownTime;
    }

    void NavigateUp()
    {
        Selectable[] currentSelectables = controlsPanel.activeSelf ? controlsSelectables : pauseSelectables;
        
        if (currentSelectables.Length == 0) return;

        currentSelection--;
        if (currentSelection < 0)
            currentSelection = currentSelectables.Length - 1;

        SelectItem(currentSelection, currentSelectables);
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(UpSFX);
            
        inputCooldown = cooldownTime;
    }

    void SelectItem(int index, Selectable[] selectables)
    {
        if (selectables.Length == 0) return;

        EventSystem.current.SetSelectedGameObject(selectables[index].gameObject);
        lastSelected = selectables[index].gameObject;
    }
    
    public void Resume()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
    
        if (pausePanel != null)
            pausePanel.SetActive(true);
        if (controlsPanel != null)
            controlsPanel.SetActive(false);
        
        if (dof != null)
            dof.active = false;
        
        Time.timeScale = 1f;
    
        if (BackgroundMusicManager.Instance != null)
            BackgroundMusicManager.Instance.PauseMenu(false);
        
        GamePaused = false;
    }

    void Pause()
    {   
        if (dof != null)
            dof.active = true;
        
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);
        
        Time.timeScale = 0f;
    
        if (BackgroundMusicManager.Instance != null)
            BackgroundMusicManager.Instance.PauseMenu(true);
        
        GamePaused = true;
    
        currentSelection = 0;
        if (defaultPauseSelectable != null)
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(defaultPauseSelectable.gameObject);
                lastSelected = defaultPauseSelectable.gameObject;
            }
        }
        else if (pauseSelectables.Length > 0)
        {
            SelectItem(0, pauseSelectables);
        }
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        GamePaused = false;
        FadeManager.Instance.FadeToScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        GamePaused = false;
        string currentSceneName = SceneManager.GetActiveScene().name;
        FadeManager.Instance.FadeToScene(currentSceneName);
    }
    
    public void OpenControls()
    {   
        if (pausePanel != null)
            pausePanel.SetActive(false);
            
        if (controlsPanel != null)
            controlsPanel.SetActive(true);
        
        currentSelection = 0;
        if (defaultControlsSelectable != null)
        {
            EventSystem.current.SetSelectedGameObject(defaultControlsSelectable.gameObject);
            lastSelected = defaultControlsSelectable.gameObject;
        }
        else if (controlsSelectables.Length > 0)
        {
            SelectItem(0, controlsSelectables);
        }
    }

    public void CloseControls()
    {   
        if (controlsPanel != null)
            controlsPanel.SetActive(false);
            
        if (pausePanel != null)
            pausePanel.SetActive(true);
        
        currentSelection = 0;
        if (defaultPauseSelectable != null)
        {
            EventSystem.current.SetSelectedGameObject(defaultPauseSelectable.gameObject);
            lastSelected = defaultPauseSelectable.gameObject;
        }
        else if (pauseSelectables.Length > 0)
        {
            SelectItem(0, pauseSelectables);
        }
    }
}