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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Also make the canvas persist
            if (fadeCanvas != null)
                DontDestroyOnLoad(fadeCanvas.gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Make sure fade image starts invisible
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
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

        // Ensure fully transparent
        color.a = 0f;
        fadeImage.color = color;

        isFading = false;
    }
}