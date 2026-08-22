using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class EvilComputer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Lever lever;
    [SerializeField] private Button3D button;

    [SerializeField] private GameObject screenObject;

    private Renderer screenRenderer;

    [Header("Side Screens")]
    [SerializeField] private List<GameObject> sideScreens;

    private List<Renderer> sideRenderers = new List<Renderer>();

    [Header("Side Screen Materials")]
    [SerializeField] private Material cheeseExe;
    [SerializeField] private Material rebooting;
    [SerializeField] private Material success;

    [Header("Main Screen Materials")]
    [SerializeField] private Material noSelected;
    [SerializeField] private Material yesSelected;
    [SerializeField] private Material pressAnywhereToStart;
    [SerializeField] private Material loading;
    [SerializeField] private Material complete;

    private enum ScreenState
    {
        NoSelected,
        YesSelected,
        PressAnywhereToStart,
        Loading,
        Complete
    }

    private ScreenState currentState;

    private bool lastLeverState;
    private bool lastButtonState;

    private void Awake()
    {
        if (screenObject != null)
        {
            screenRenderer = screenObject.GetComponent<Renderer>();
        }

        sideRenderers.Clear();
        foreach (var obj in sideScreens)
        {
            if (obj == null) continue;

            var r = obj.GetComponent<Renderer>();
            if (r != null)
                sideRenderers.Add(r);
        }
    }

    private void OnEnable()
    {
        StartCoroutine(SetSideScreensMaterialSequential(cheeseExe));
    }

    private void Start()
    {
        lastLeverState = lever.IsActive;
        lastButtonState = button.IsActive;
        SetScreen(ScreenState.NoSelected);
    }

    private void Update()
    {
        CheckLever();
        CheckButton();
    }

    private void CheckLever()
    {
        if (lever.IsActive != lastLeverState)
        {
            lastLeverState = lever.IsActive;

            if (currentState == ScreenState.NoSelected)
                SetScreen(ScreenState.YesSelected);
            else if (currentState == ScreenState.YesSelected)
                SetScreen(ScreenState.NoSelected);
        }
    }

    private void CheckButton()
    {
        if (button.IsActive != lastButtonState)
        {
            lastButtonState = button.IsActive;

            if (button.IsActive)
            {
                switch (currentState)
                {
                    case ScreenState.NoSelected:
                        SetScreen(ScreenState.PressAnywhereToStart);
                        break;

                    case ScreenState.YesSelected:
                        StartCoroutine(GoToLoadingSequence());
                        break;

                    case ScreenState.PressAnywhereToStart:
                        if (lever.IsActive)
                            SetScreen(ScreenState.NoSelected);
                        else
                            SetScreen(ScreenState.YesSelected);
                        break;
                }
            }
        }
    }

    private IEnumerator GoToLoadingSequence()
    {   
        SetScreen(ScreenState.Loading);
        yield return StartCoroutine(SetSideScreensMaterialSequential(rebooting));

        yield return new WaitForSeconds(4f);

        SetScreen(ScreenState.Complete);
        yield return StartCoroutine(SetSideScreensMaterialSequential(success));

        yield return new WaitForSeconds(4f);

        if (FadeManager.Instance != null)
            FadeManager.Instance.FadeToScene("EndScreen");
        else
            SceneManager.LoadScene("EndScreen");
    }

    private void SetScreen(ScreenState state)
    {
        currentState = state;

        if (screenRenderer == null) return;

        switch (state)
        {
            case ScreenState.NoSelected:
                screenRenderer.material = noSelected;
                break;

            case ScreenState.YesSelected:
                screenRenderer.material = yesSelected;
                break;

            case ScreenState.PressAnywhereToStart:
                screenRenderer.material = pressAnywhereToStart;
                break;

            case ScreenState.Loading:
                screenRenderer.material = loading;
                break;

            case ScreenState.Complete:
                screenRenderer.material = complete;
                break;
        }
    }

    private IEnumerator SetSideScreensMaterialSequential(Material mat)
    {
        foreach (var r in sideRenderers)
        {
            if (r != null)
                r.material = mat;

            yield return new WaitForSeconds(0.1f);
        }
    }
}