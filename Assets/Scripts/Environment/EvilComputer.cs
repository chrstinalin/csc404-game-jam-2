using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class EvilComputer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Lever lever;
    [SerializeField] private Button3D button;

    [SerializeField] private GameObject screenObject;

    private Renderer screenRenderer;

    [Header("Materials")]
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

            if (screenRenderer == null)
            {
                Debug.LogError("Screen object has no Renderer component!");
            }
        }
        else
        {
            Debug.LogError("Screen object is not assigned!");
        }
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
                        {
                            SetScreen(ScreenState.NoSelected);
                        }
                        else
                        {
                            SetScreen(ScreenState.YesSelected);
                        }
                        break;
                }
            }
        }
    }

    private IEnumerator GoToLoadingSequence()
    {
        SetScreen(ScreenState.Loading);

        yield return new WaitForSeconds(1.5f);

        SetScreen(ScreenState.Complete);
        yield return new WaitForSeconds(3f);

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
}