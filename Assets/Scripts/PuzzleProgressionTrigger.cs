using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;

public class PuzzleProgressionTrigger : MonoBehaviour
{
    [Header("Scene Settings")]
    public float delayBeforeLoad = 0.5f;
    public string nextSceneName;

    [Header("FMOD Events")]
    [SerializeField] public EventReference itemEnterSFX;

    [Header("Triggerables")]
    [SerializeField] public List<TriggerableAbstract> triggerables;

    private bool sceneLoading = false;
    private bool iconsLocked = false;

    [SerializeField] private GameObject floatingCube;
    private Transform camTransform;

    [SerializeField] private GameObject mouseIcon;
    [SerializeField] private GameObject mechIcon;
    [SerializeField] private GameObject cheeseIcon;

    [SerializeField] private float fadeDuration = 0.3f;
    private Coroutine mouseFadeRoutine;
    private Coroutine mechFadeRoutine;
    private Coroutine cheeseFadeRoutine;

    private bool requiresMouse;
    private bool requiresMech;
    private bool requiresCheese;

    private bool mouseEntered = false;
    private bool mechEntered = false;
    private bool cheeseEntered = false;

    private bool allEnteredSFXPlayed = false;

    private void Start()
    {
        camTransform = CameraManager.Instance.transform;

        requiresMouse = mouseIcon != null;
        requiresMech = mechIcon != null;
        requiresCheese = cheeseIcon != null;
    }

    private void Update() => HandleBillboard();

    private void HandleBillboard()
    {
        if (floatingCube == null || camTransform == null) return;

        Vector3 direction = camTransform.position - floatingCube.transform.position;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(-direction) * Quaternion.Euler(0f, 90f, 0f);

            floatingCube.transform.rotation = targetRotation;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        bool triggered = false;

        if (requiresMech && PlayerMech.Instance != null &&
            other.transform.IsChildOf(PlayerMech.Instance.transform))
        {
            if (!mechEntered)
            {
                mechEntered = true;
                FadeIcon(mechIcon, ref mechFadeRoutine, 0.06f);
                triggered = true;
            }
        }

        if (requiresMouse && PlayerMouse.Instance != null &&
            other.transform.IsChildOf(PlayerMouse.Instance.transform))
        {
            if (!mouseEntered)
            {
                mouseEntered = true;
                FadeIcon(mouseIcon, ref mouseFadeRoutine, 0.06f);
                triggered = true;
            }
        }

        if (requiresCheese && other.CompareTag("Cheese"))
        {
            if (!cheeseEntered)
            {
                cheeseEntered = true;
                FadeIcon(cheeseIcon, ref cheeseFadeRoutine, 0.06f);
                triggered = true;
            }
        }

        if (triggered)
            AudioManager.Instance.PlaySFX(itemEnterSFX, transform.position, 1f);

        HandleStateChange();
    }

    private void OnTriggerExit(Collider other)
    {
        if (requiresMech && PlayerMech.Instance != null &&
            other.transform.IsChildOf(PlayerMech.Instance.transform))
        {
            mechEntered = false;
            FadeIcon(mechIcon, ref mechFadeRoutine, 1f);
        }

        if (requiresMouse && PlayerMouse.Instance != null &&
            other.transform.IsChildOf(PlayerMouse.Instance.transform))
        {
            mouseEntered = false;
            FadeIcon(mouseIcon, ref mouseFadeRoutine, 1f);
        }

        if (requiresCheese && other.CompareTag("Cheese"))
        {
            cheeseEntered = false;
            FadeIcon(cheeseIcon, ref cheeseFadeRoutine, 1f);
        }

        HandleStateChange();
    }

    private void FadeIcon(GameObject icon, ref Coroutine routine, float targetAlpha)
    {
        if (iconsLocked || icon == null) return;

        SpriteRenderer sr = icon.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(FadeSprite(sr, targetAlpha));
    }

    private IEnumerator FadeSprite(SpriteRenderer sr, float targetAlpha)
    {
        float startAlpha = sr.color.a;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            sr.color = new Color(
                sr.color.r,
                sr.color.g,
                sr.color.b,
                Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration)
            );

            yield return null;
        }

        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, targetAlpha);
    }

    private void HandleStateChange()
    {
        if (!requiresMouse && !requiresMech && !requiresCheese)
            return;

        bool allMet =
            (!requiresMech || mechEntered) &&
            (!requiresMouse || mouseEntered) &&
            (!requiresCheese || cheeseEntered);

        if (allMet && !sceneLoading)
        {
            iconsLocked = true;

            SetIconAlpha(mouseIcon, 0.06f);
            SetIconAlpha(mechIcon, 0.06f);
            SetIconAlpha(cheeseIcon, 0.06f);

            if (!allEnteredSFXPlayed)
            {
                AudioManager.Instance.PlaySFX(
                    AudioManager.Instance.loadNextPuzzleSFX,
                    transform.position,
                    1f
                );

                allEnteredSFXPlayed = true;
            }

            sceneLoading = true;

            foreach (var t in triggerables)
            {
                t.TurnOn();
            }

            if (triggerables.Count == 0)
            {
                Invoke(nameof(TriggerFade), delayBeforeLoad);
            }
        }
        else
        {
            allEnteredSFXPlayed = false;
        }
    }

    private void TriggerFade()
    {
        if (FadeManager.Instance == null) return;

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            FadeManager.Instance.FadeToScene(nextSceneName);
        }
        else
        {
            int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
            FadeManager.Instance.FadeToScene(nextIndex.ToString());
        }
    }

    private void SetIconAlpha(GameObject icon, float alpha)
    {
        if (icon == null) return;

        var sr = icon.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
    }
}