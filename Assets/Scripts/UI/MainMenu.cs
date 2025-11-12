using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Audio;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Buttons")]
    public UnityEngine.UI.Button[] menuButtons;
    
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject controlsPanel;
    
    private int currentSelection = 0;
    private float inputCooldown = 0f;
    private float cooldownTime = 0.2f;
    private GameObject lastSelected;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioSource selectSound;

    public AudioClip moveUpClip;
    public AudioClip moveDownClip;
    public AudioClip introClip;
    public AudioClip mainTheme;

    void Start()
    {
        if (menuButtons.Length > 0)
        {
            EventSystem.current.SetSelectedGameObject(menuButtons[0].gameObject);
            lastSelected = menuButtons[0].gameObject;
        }

        audioSource.clip = introClip;
        audioSource.Play();
        Invoke("PlayMusic", introClip.length);
    }

    void Update()
    {
        HandleControllerInput();
        PreventMouseDeselection();
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

        if (controlsPanel.activeSelf)
        {
            if (Input.GetButtonDown("Submit"))
            {
                PlaySelectSound();
                CloseControls();
                inputCooldown = cooldownTime;
            }
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

            PlaySound(moveDownClip);
        }
        else if (vertical > 0.5f)
        {
            currentSelection--;
            if (currentSelection < 0)
                currentSelection = menuButtons.Length - 1;
            
            EventSystem.current.SetSelectedGameObject(menuButtons[currentSelection].gameObject);
            lastSelected = menuButtons[currentSelection].gameObject;
            inputCooldown = cooldownTime;

            PlaySound(moveUpClip);
        }

        if (Input.GetButtonDown("Submit"))
        {
            PlaySelectSound();
            switch (currentSelection)
            {
                case 0:
                    PlayGame();
                    break;
                case 1:
                    OpenControls();
                    break;
                case 2:
                    QuitGame();
                    break;
            }
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Puzzle4");
    }

    public void OpenControls()
    {
        mainMenuPanel.SetActive(false);
        controlsPanel.SetActive(true);
    }

    public void CloseControls()
    {
        controlsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        currentSelection = 0;
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

    public void PlayMusic()
    {
        audioSource.clip = mainTheme;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
    
    public void PlaySelectSound()
    {
        selectSound.Play();
    }
}