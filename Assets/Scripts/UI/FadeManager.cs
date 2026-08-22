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
    private Coroutine fadeRoutine;

    public static bool IsFading => Instance != null && Instance.isFading;

    void Awake()
    {
        // This component sits on a CHILD of the FadeCanvas root, so anything that
        // should apply to the whole fade overlay has to target the canvas, not us.
        GameObject root = fadeCanvas != null ? fadeCanvas.gameObject : gameObject;

        if (Instance != null && Instance != this)
        {
            root.SetActive(false);
            Destroy(root);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(root);

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
        if (isFading) return;

        fadeRoutine = StartCoroutine(FadeOutInRoutine(sceneName));
    }

    IEnumerator FadeOutInRoutine(string sceneName)
    {
        isFading = true;

        yield return StartCoroutine(Fade(0f, 1f));

        // Fade to black
        float timer = 0f;
        Color color = fadeImage.color;
        fadeImage.raycastTarget = true;

        op.allowSceneActivation = true;

        while (!op.isDone)
        {
            yield return null;
        }

        yield return new WaitForEndOfFrame();
        yield return null;

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
        fadeRoutine = null;
    }

    IEnumerator Fade(float from, float to)
    {
        if (fadeImage == null)
        {
            Debug.LogError("FadeManager: fadeImage missing!");
            yield break;
        }

        float t = 0f;

        Color c = fadeImage.color;
        c.a = from;
        fadeImage.color = c;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;

            c.a = Mathf.Lerp(from, to, t / fadeDuration);
            fadeImage.color = c;

            yield return null;
        }

        c.a = to;
        fadeImage.color = c;
    }

    void SetAlpha(float a)
    {
        if (fadeImage == null) return;

        Color c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
    }
}