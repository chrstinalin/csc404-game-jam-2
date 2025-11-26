using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadNextPuzzle : MonoBehaviour
{
    [Header("Scene Settings")]
    public float delayBeforeLoad = 0.5f; // Optional small delay
    public string nextSceneName;       // Leave empty to load next scene in build order

    private bool mechEntered = false;
    private bool mouseEntered = false;
    private bool sceneLoading = false;

    public GameObject textPrefab;
    private GameObject spawnedText;
     private Coroutine warningRoutine;

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

        HandleStateChange();

        if (mechEntered && mouseEntered && !sceneLoading)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.loadNextPuzzleSFX, transform.position, 1f);
            sceneLoading = true;
            Invoke(nameof(LoadNextScene), delayBeforeLoad);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (PlayerMech.Instance != null &&
            other.transform.IsChildOf(PlayerMech.Instance.transform))
        {
            mechEntered = false;
        }

        if (PlayerMouse.Instance != null &&
            other.transform.IsChildOf(PlayerMouse.Instance.transform))
        {
            mouseEntered = false;
        }

        HandleStateChange();
    }

    private void HandleStateChange()
    {
        if (mechEntered && mouseEntered && !sceneLoading)
        {
            CancelWarning();
            sceneLoading = true;
            Invoke(nameof(LoadNextScene), delayBeforeLoad);
            return;
        }

        if (mechEntered ^ mouseEntered)
        {
            if (warningRoutine == null)
                warningRoutine = StartCoroutine(ShowWarningAfterDelay());
        }
        else
        {
            // If none or both → cancel warning and hide text
            CancelWarning();
        }
    }

    private IEnumerator ShowWarningAfterDelay()
    {

        yield return new WaitForSeconds(3f);

        if (mechEntered ^ mouseEntered)
        {
        if (spawnedText == null)
            {
                Canvas canvas = FindObjectOfType<Canvas>();
                spawnedText = Instantiate(textPrefab, canvas.transform);
                spawnedText.GetComponent<Text>().text =
                    "Both Peanut and Dreadnought Killer must enter to progress.";
                var follower = spawnedText.GetComponent<InteractableObjectText>();
                follower.target = transform;
            }
        }

        warningRoutine = null;
    }

    private void CancelWarning()
    {
        if (warningRoutine != null)
        {
            StopCoroutine(warningRoutine);
            warningRoutine = null;
        }

        if (spawnedText != null)
        {
            Destroy(spawnedText);
            spawnedText = null;
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
