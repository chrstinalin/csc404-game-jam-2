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

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (fadeCanvas != null)
                DontDestroyOnLoad(fadeCanvas.gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SetAlpha(0f);
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

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

        op.allowSceneActivation = true;

        while (!op.isDone)
        {
            yield return null;
        }

        yield return new WaitForEndOfFrame();
        yield return null;

        yield return StartCoroutine(Fade(1f, 0f));

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