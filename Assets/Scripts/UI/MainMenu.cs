using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Buttons")]
    public UnityEngine.UI.Button[] menuButtons;
    
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;
    
    [Header("Input Settings")]
    public bool disableMouseInput = true;
    
    private int currentSelection = 0;
    private float inputCooldown = 0f;
    private float cooldownTime = 0.2f;
    private GameObject lastSelected;

    void Start()
    {
        if (menuButtons.Length > 0)
        {
            EventSystem.current.SetSelectedGameObject(menuButtons[0].gameObject);
            lastSelected = menuButtons[0].gameObject;
        }
    }

    void Update()
    {
        HandleControllerInput();
        if (disableMouseInput) PreventMouseDeselection();
    }

    void PreventMouseDeselection()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(lastSelected);
        }
        else
        {
            lastSelected = EventSystem.current.currentSelectedGameObject;
        }
    }

    void HandleControllerInput()
    {
        if (inputCooldown > 0)
        {
            inputCooldown -= Time.deltaTime;
            return;
        }

        float vertical = Input.GetAxisRaw("Vertical");

        if (vertical < -0.5f)
        {
            currentSelection++;
            if (currentSelection >= menuButtons.Length)
                currentSelection = 0;
            
            EventSystem.current.SetSelectedGameObject(menuButtons[currentSelection].gameObject);
            lastSelected = menuButtons[currentSelection].gameObject;
            inputCooldown = cooldownTime;
        }
        else if (vertical > 0.5f)
        {
            currentSelection--;
            if (currentSelection < 0)
                currentSelection = menuButtons.Length - 1;
            
            EventSystem.current.SetSelectedGameObject(menuButtons[currentSelection].gameObject);
            lastSelected = menuButtons[currentSelection].gameObject;
            inputCooldown = cooldownTime;
        }

        if (Input.GetButtonDown("Submit"))
        {
            switch (currentSelection)
            {
                case 0:
                    PlayGame();
                    break;
                case 1:
                    OpenOptions();
                    break;
                case 2:
                    QuitGame();
                    break;
            }
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("DefaultScene");
    }

    public void OpenOptions()
    {
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(menuButtons[0].gameObject);
        lastSelected = menuButtons[0].gameObject;
    }

    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}