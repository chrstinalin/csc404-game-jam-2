using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using FMODUnity;
using TMPro;

public class UIManager : MonoBehaviour
{
    [NonSerialized] private Health MouseHealth;
    [NonSerialized] private Image[] HealthPoints;
    [NonSerialized] private Health MechHealth;
    [NonSerialized] private Image HealthFront;
    [NonSerialized] private Image HealthBack;

    [SerializeField] private TextMeshProUGUI MechName;
    [SerializeField] private TextMeshProUGUI MouseName;
    private Color originalMechNameColor;
    private Color originalMouseNameColor;

    [NonSerialized] private float lerpTimer;
    [NonSerialized] private float DAMAGE_LERP = 5f;
    [NonSerialized] private float CHIP_LERP = 50f;

    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private Volume volume;
    private DepthOfField dof;

    public static bool GamePaused = false;

    [SerializeField] private Selectable[] pauseSelectables;
    [SerializeField] private Selectable defaultPauseSelectable;
    [SerializeField] private Selectable[] controlsSelectables;
    [SerializeField] private Selectable defaultControlsSelectable;

    private int currentSelection = 0;
    private float inputCooldown = 0f;
    private float cooldownTime = 0.2f;
    private GameObject lastSelected;

    [SerializeField] private EventReference SelectSFX;
    [SerializeField] private EventReference UpSFX;
    [SerializeField] private EventReference DownSFX;
    [SerializeField] private EventReference BackSFX;

    private ControlSchemeManager controlSchemeManager;
    private GameObject mouseControlsUI;
    private GameObject mechControlsUI;

    [Header("In-Game Control Text")]
    [SerializeField] private Text mouseControlsText;
    [SerializeField] private Text mechControlsText;

    [Header("Lock-On Alternative Sprites")]
    [SerializeField] private Sprite[] lockOnHealthPoints;
    [SerializeField] private Sprite lockOnHealthFront;   
    [SerializeField] private Sprite lockOnHealthBack; 

    [Header("Lock-On Border Sprites")]
    [SerializeField] private GameObject MechHealthBorderObj; 
    [SerializeField] private GameObject MouseHealthBorderObj; 
    [SerializeField] private Sprite lockOnMechBorder;
    [SerializeField] private Sprite lockOnMouseBorder;

    private Sprite[] originalHealthPoints;
    private Sprite originalHealthFront;
    private Sprite originalHealthBack;

    private Sprite originalMechBorder;
    private Sprite originalMouseBorder;
    private Image mechBorderImage;
    private Image mouseBorderImage;

    private void Start()
    {
        InitializeHealthUI();

        if (MechName != null) originalMechNameColor = MechName.color;
        if (MouseName != null) originalMouseNameColor = MouseName.color;

        InitializePauseMenu();
        InitializeControlSchemeManager();
        UpdateInGameControlText();
    }

    private void InitializeHealthUI()
    {
        GameObject HealthPointContainer = GameObject.FindGameObjectWithTag("MouseHealthPointContainer");
        GameObject _HealthFront = GameObject.FindGameObjectWithTag("MechHealthFront");
        GameObject _HealthBack = GameObject.FindGameObjectWithTag("MechHealthBack");

        mouseControlsUI = GameObject.FindGameObjectWithTag("MouseControls");
        mechControlsUI = GameObject.FindGameObjectWithTag("MechControls");

        if (!HealthPointContainer || !_HealthFront || !_HealthBack)
        {
            Debug.LogError("UI elements not found.");
            return;
        }

        HealthPoints = HealthPointContainer.GetComponentsInChildren<Image>();

        // Store original mouse health point sprites
        originalHealthPoints = new Sprite[HealthPoints.Length];
        for (int i = 0; i < HealthPoints.Length; i++)
            originalHealthPoints[i] = HealthPoints[i].sprite;

        HealthFront = _HealthFront.GetComponent<Image>();
        HealthBack = _HealthBack.GetComponent<Image>();

        // Store original mech health sprites
        if (HealthFront != null) originalHealthFront = HealthFront.sprite;
        if (HealthBack != null) originalHealthBack = HealthBack.sprite;

        // Borders
        if (MechHealthBorderObj != null)
            mechBorderImage = MechHealthBorderObj.GetComponent<Image>();
        if (mechBorderImage != null)
            originalMechBorder = mechBorderImage.sprite;

        if (MouseHealthBorderObj != null)
            mouseBorderImage = MouseHealthBorderObj.GetComponent<Image>();
        if (mouseBorderImage != null)
            originalMouseBorder = mouseBorderImage.sprite;

        if (PlayerMouse.Instance != null)
        {
            MouseHealth = PlayerMouse.Instance.GetComponent<Health>();
            if (MouseHealth != null)
                MouseHealth.onHealthChanged.AddListener(OnMouseHealthChanged);
        }

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
        if (volume != null && volume.profile.TryGet(out dof))
            dof.active = false;

        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
        if (controlsPanel != null) controlsPanel.SetActive(false);

        GamePaused = false;
    }

    private void InitializeControlSchemeManager()
    {
        controlSchemeManager = FindObjectOfType<ControlSchemeManager>();
        if (controlSchemeManager == null)
            Debug.LogWarning("ControlSchemeManager not found in scene.");
    }

    private void UpdateInGameControlText()
    {
        if (controlSchemeManager == null) return;

        ControllerType currentController = controlSchemeManager.GetCurrentControllerType();
        bool isLockedOn = LockOnManager.Instance != null && LockOnManager.Instance.IsLockedOn;

        if (mouseControlsText != null)
            mouseControlsText.text = BuildMouseControlsText(currentController);

        if (mechControlsText != null)
        {
            if (isLockedOn)
                mechControlsText.text = BuildMechLockOnText(currentController);
            else
                mechControlsText.text = BuildMechControlsText(currentController);
        }
    }
    private string BuildMouseControlsText(ControllerType controller)
    {
        string interact = GetButtonOnly(ActionType.Interact, controller);
        string jump = GetButtonOnly(ActionType.Jump, controller);
        string switchChar = GetButtonOnly(ActionType.SwitchCharacter, controller);
        string lockon = GetButtonOnly(ActionType.Lockon, controller);

        return $"JUMP ({jump})\nINTERACT ({interact})\nSUMMON MECH ({lockon})\nSWITCH CHARACTERS ({switchChar})";
    }

    private string BuildMechControlsText(ControllerType controller)
    {
        string interact = GetButtonOnly(ActionType.Interact, controller);
        string lockon = GetButtonOnly(ActionType.Lockon, controller);
        string switchChar = GetButtonOnly(ActionType.SwitchCharacter, controller);

        return $"INTERACT ({interact})\nENTER LOCK-ON MODE ({lockon})\nSWITCH CHARACTERS ({switchChar})";
    }

    private string BuildMechLockOnText(ControllerType controller)
    {
        string switchTarget = GetButtonOnly(ActionType.ChangeLockOnTarget, controller);
        string shoot = GetButtonOnly(ActionType.Shoot, controller);

        return $"SWITCH TARGET ({switchTarget})\nSHOOT ENEMY ({shoot})";
    }

    private string GetButtonOnly(ActionType action, ControllerType controller)
    {
        string fullLabel = ControlSchemeManager.GetButtonLabel(action, controller);
        int colonIndex = fullLabel.IndexOf(':');
        if (colonIndex > 0) return fullLabel.Substring(0, colonIndex).Trim();
        return fullLabel;
    }

    private void Update()
    {
        updateMechHealthUI();
        HandlePauseInput();
        UpdateControlVisibility();
        UpdateInGameControlText();
        HandleLockOnSprites();

        if (GamePaused)
            HandlePauseMenuNavigation();
    }

    private void UpdateControlVisibility()
    {
        if (mouseControlsText != null && mouseControlsUI != null)
            mouseControlsText.enabled = mouseControlsUI.activeSelf;

        if (mechControlsText != null && mechControlsUI != null)
            mechControlsText.enabled = mechControlsUI.activeSelf;
    }
    private void HandleLockOnSprites()
    {
        bool isLockedOn = LockOnManager.Instance != null && LockOnManager.Instance.IsLockedOn;

        // Swap Mouse Health Points
        if (HealthPoints != null && lockOnHealthPoints != null)
        {
            for (int i = 0; i < HealthPoints.Length; i++)
            {
                if (i < lockOnHealthPoints.Length)
                    HealthPoints[i].sprite = isLockedOn ? lockOnHealthPoints[i] : originalHealthPoints[i];
            }
        }

        // Swap Mech Health Front & Back
        if (HealthFront != null)
            HealthFront.sprite = isLockedOn && lockOnHealthFront != null ? lockOnHealthFront : originalHealthFront;
        if (HealthBack != null)
            HealthBack.sprite = isLockedOn && lockOnHealthBack != null ? lockOnHealthBack : originalHealthBack;

        // Swap Border Images
        if (mechBorderImage != null)
            mechBorderImage.sprite = isLockedOn && lockOnMechBorder != null ? lockOnMechBorder : originalMechBorder;

        if (mouseBorderImage != null)
            mouseBorderImage.sprite = isLockedOn && lockOnMouseBorder != null ? lockOnMouseBorder : originalMouseBorder;

        // Change character name text to white
        if (MechName != null) MechName.color = isLockedOn ? Color.white : originalMechNameColor;
        if (MouseName != null) MouseName.color = isLockedOn ? Color.white : originalMouseNameColor;
    }

    private void updateMechHealthUI()
    {
        if (!HealthFront || !HealthBack || !MechHealth) return;

        float fillA = HealthFront.fillAmount;
        float fillB = HealthBack.fillAmount;
        float hFraction = (float)MechHealth.GetCurrHealth() / MechHealth.GetMaxHealth();

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
                HealthPoints[i].enabled = i <= MouseHealth.GetCurrHealth() - 1;
        }
    }

    public void SetActiveCharacterUI(bool isMouseActive)
    {
        if (mouseControlsUI != null) mouseControlsUI.SetActive(isMouseActive);
        if (mechControlsUI != null) mechControlsUI.SetActive(!isMouseActive);
        UpdateInGameControlText();
    }

    // Only opens the menu. Closing goes through HandlePauseMenuNavigation, so
    // that Pause and Cancel both back out of the controls panel first, and so
    // Escape - which is both - is never acted on twice in the same frame.
    private void HandlePauseInput()
    {
        if (FadeManager.IsFading)
            return;

        if (!GamePaused && GameInput.PauseDown) Pause();
    }
    
    private void HandlePauseMenuNavigation()
    {
        if (inputCooldown > 0)
        {
            inputCooldown -= Time.unscaledDeltaTime;
            return;
        }

        if (GameInput.CancelDown || GameInput.PauseDown)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(BackSFX);

            if (controlsPanel != null && controlsPanel.activeSelf) CloseControls();
            else Resume();

            inputCooldown = cooldownTime;
            return;
        }

        float vertical = GameInput.MenuVertical;
        if (vertical < -0.5f) NavigateDown();
        else if (vertical > 0.5f) NavigateUp();

        if (GameInput.SubmitDown)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(SelectSFX);

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
        if (currentSelection >= currentSelectables.Length) currentSelection = 0;

        SelectItem(currentSelection, currentSelectables);

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(DownSFX);
        inputCooldown = cooldownTime;
    }

    void NavigateUp()
    {
        Selectable[] currentSelectables = controlsPanel.activeSelf ? controlsSelectables : pauseSelectables;
        if (currentSelectables.Length == 0) return;

        currentSelection--;
        if (currentSelection < 0) currentSelection = currentSelectables.Length - 1;

        SelectItem(currentSelection, currentSelectables);

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(UpSFX);
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
        if (MovementManager.Instance != null) MovementManager.Instance.unlockInput();
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (dof != null) dof.active = false;

        Time.timeScale = 1f;
        if (BackgroundMusicManager.Instance != null) BackgroundMusicManager.Instance.PauseMenu(false);

        GamePaused = false;
    }

    void Pause()
    {
        if (MovementManager.Instance != null) MovementManager.Instance.lockInput();
        if (dof != null) dof.active = true;
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);

        Time.timeScale = 0f;
        if (BackgroundMusicManager.Instance != null) BackgroundMusicManager.Instance.PauseMenu(true);

        GamePaused = true;
        currentSelection = 0;

        // Escape opens the menu and is also Cancel: without this the same key
        // press would close it again on the very frame it opened.
        inputCooldown = cooldownTime;

        GameInput.TakeOverMenuSubmit();

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

        if (controlSchemeManager != null)
            controlSchemeManager.ForceUpdate();
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        GamePaused = false;

        if (FadeManager.Instance != null)
            FadeManager.Instance.FadeToScene("MainMenu");
        else
            SceneManager.LoadScene("MainMenu");
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

        if (FadeManager.Instance != null)
            FadeManager.Instance.FadeToScene(currentSceneName);
        else
            SceneManager.LoadScene(currentSceneName);
    }

    public void OpenControls()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(true);

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

        if (controlSchemeManager != null)
            controlSchemeManager.ForceUpdate();
    }

    public void CloseControls()
    {
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);

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