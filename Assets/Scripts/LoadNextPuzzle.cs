using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNextSceneOnBothCollideChildren : MonoBehaviour
{
    [Header("Scene Settings")]
    public float delayBeforeLoad = 1f; // Optional small delay
    public string nextSceneName;       // Leave empty to load next scene in build order

    private bool mechEntered = false;
    private bool mouseEntered = false;
    private bool sceneLoading = false;

    private void OnTriggerEnter(Collider other)
    {
        if (PlayerMech.Instance != null &&
            other.gameObject.transform.IsChildOf(PlayerMech.Instance.transform))
        {
            mechEntered = true;
        }

        if (PlayerMouse.Instance != null &&
            other.gameObject.transform.IsChildOf(PlayerMouse.Instance.transform))
        {
            mouseEntered = true;
        }

        if (mechEntered && mouseEntered && !sceneLoading)
        {
            sceneLoading = true;
            Invoke(nameof(LoadNextScene), delayBeforeLoad);
        }
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
