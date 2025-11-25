using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadNextSceneOnBothCollideChildren : MonoBehaviour
{
    [Header("Scene Settings")]
    public float delayBeforeLoad = 1f; // Optional small delay
    public string nextSceneName;       // Leave empty to load next scene in build order

    private bool mechEntered = false;
    private bool mouseEntered = false;
    private bool sceneLoading = false;

    public GameObject textPrefab;
    private GameObject spawnedText;

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
        Canvas canvas = FindObjectOfType<Canvas>();

        if ((mechEntered && !mouseEntered) || (!mechEntered && mouseEntered))
        {
            if (spawnedText != null)
            {
                Destroy(spawnedText);
            }

            spawnedText = Instantiate(textPrefab, canvas.transform);


            var textComponent = spawnedText.GetComponent<Text>();
            textComponent.text = "Both Peanut and Dreadnought Killer must enter the door.";

            var follower = spawnedText.GetComponent<InteractableObjectText>();
            follower.target = transform;
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
