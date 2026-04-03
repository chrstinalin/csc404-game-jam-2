using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class EvilComputer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Lever lever;
    [SerializeField] private Button3D button;
    [SerializeField] private SpriteRenderer screenRenderer;

    [Header("Screens")]
    [SerializeField] private Sprite noSelected;
    [SerializeField] private Sprite yesSelected;
    [SerializeField] private Sprite pressAnywhereToStart;
    [SerializeField] private Sprite loading;
    [SerializeField] private Sprite complete;

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
                        else if (!lever.IsActive)
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

        switch (state)
        {
            case ScreenState.NoSelected:
                screenRenderer.sprite = noSelected;
                break;

            case ScreenState.YesSelected:
                screenRenderer.sprite = yesSelected;
                break;

            case ScreenState.PressAnywhereToStart:
                screenRenderer.sprite = pressAnywhereToStart;
                break;

            case ScreenState.Loading:
                screenRenderer.sprite = loading;
                break;

            case ScreenState.Complete:
                screenRenderer.sprite = complete;
                break;
        }
    }
}