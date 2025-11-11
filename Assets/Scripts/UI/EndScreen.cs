using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour
{
    public GameObject button;
    public AudioSource audioSource;

    void Start()
    {
        EventSystem.current.SetSelectedGameObject(button);
    }

    public void LoadMainMenu()
    {
        audioSource.Play();
        SceneManager.LoadScene("MainMenu");
    }
}
