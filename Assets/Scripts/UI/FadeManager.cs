using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    [SerializeField] private Canvas fadeCanvas;
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    private bool isFading = false;

    void Awake()
    {
        // This component sits on a CHILD of the FadeCanvas root, so anything that
        // should apply to the whole fade overlay has to target the canvas, not us.
        GameObject root = fadeCanvas != null ? fadeCanvas.gameObject : gameObject;

        if (Instance != null && Instance != this)
        {
            // A FadeManager from an earlier scene already persists, so this scene's
            // own FadeCanvas is a duplicate. Destroy the whole canvas: destroying
            // only this child would strand the canvas (sorting order 100) with its
            // full-screen raycast-blocking FadeImage, and that orphan swallows every
            // click in the scene - the topmost raycast hit stops being the button.
            // Deactivate first, since Destroy only takes effect at end of frame.
            root.SetActive(false);
            Destroy(root);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(root);

        // Make sure fade image starts invisible and stops eating clicks -
        // otherwise this full-screen overlay silently blocks every raycast
        // (mouse clicks) on any UI underneath it, even at alpha 0.
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
            fadeImage.raycastTarget = false;
        }
    }

    public void FadeToScene(string sceneName)
    {
        StopAllCoroutines();
        StartCoroutine(FadeOutAndLoadScene(sceneName));
    }

    public void FadeIn()
    {
        if (!isFading)
            StartCoroutine(FadeInCoroutine());
    }

    IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        isFading = true;

        // Null check
        if (fadeImage == null)
        {
            Debug.LogError("FadeManager: fadeImage is null!");
            SceneManager.LoadScene(sceneName);
            isFading = false;
            yield break;
        }

        // Fade to black
        float timer = 0f;
        Color color = fadeImage.color;
        fadeImage.raycastTarget = true;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        // Ensure fully black
        color.a = 1f;
        fadeImage.color = color;

        // Load the scene
        SceneManager.LoadScene(sceneName);

        // Wait a frame for scene to load
        yield return new WaitForSeconds(0.1f);

        // Fade in from black
        yield return StartCoroutine(FadeInCoroutine());
    }

    IEnumerator FadeInCoroutine()
    {
        isFading = true;

        // Null check
        if (fadeImage == null)
        {
            Debug.LogError("FadeManager: fadeImage is null during fade in!");
            isFading = false;
            yield break;
        }

        float timer = 0f;
        Color color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        // Ensure fully transparent and stop blocking clicks again
        color.a = 0f;
        fadeImage.color = color;
        fadeImage.raycastTarget = false;

        isFading = false;
    }
}