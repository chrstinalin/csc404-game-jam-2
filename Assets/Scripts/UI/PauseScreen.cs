using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class PauseScreen : MonoBehaviour
{

    public static bool GamePaused = false;
    public GameObject pauseMenuUI;

    public GameObject controlsPanel;

    [SerializeField] private Button firstSelectedButton;

    [SerializeField] private Volume volume;
    private DepthOfField dof;

    void Start()
    {
        volume.profile.TryGet(out dof);
        dof.active = false;
        EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
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

        if (controlsPanel.activeSelf)
        {
            if (Input.GetButtonDown("Submit"))
            {
                CloseControls();
            }
            return;
        }
    }
        
    public void Resume ()
    {
        BackgroundMusicManager.Instance.PauseMenu(false);
        pauseMenuUI.SetActive(false);
        dof.active = false;
        Time.timeScale = 1f;
        GamePaused = false;
    }

    void Pause()
    {   
        BackgroundMusicManager.Instance.PauseMenu(true);
        dof.active = enabled;
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GamePaused = true;
    }

    public void LoadMainMenu()
    {
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
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
    
    public void OpenControls()
    {   
        pauseMenuUI.SetActive(false);
        controlsPanel.SetActive(true);
    }

    public void CloseControls()
    {   
        pauseMenuUI.SetActive(true);
        controlsPanel.SetActive(false);
    }
    
}