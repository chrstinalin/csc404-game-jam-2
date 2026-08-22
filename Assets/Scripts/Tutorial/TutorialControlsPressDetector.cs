using UnityEngine;

public class TutorialControlsPressDetector: MonoBehaviour
{
    [Header("Tutorial Manager")]
    public TutorialManager tutorialManager;

    private bool switchedToPeanutDone = false;
    private bool activatedLockOnMode = false;

    public void CheckSwitchToPeanut()
    {
        if (switchedToPeanutDone)
            return;

        if (GameInput.SwitchCharacterDown)
        {
            switchedToPeanutDone = true;

            tutorialManager.TriggerMilestone(TutorialManager.Milestone.SwitchedToPeanut);
        }
    }

    public void CheckActivateLockOnMode()
    {
        if (activatedLockOnMode)
            return;

        if (GameInput.LockOnModeDown)
        {

            tutorialManager.TriggerMilestone(TutorialManager.Milestone.ActivateLockOnMode);
        }
    }

    private void Update()
    {
        CheckSwitchToPeanut();
        CheckActivateLockOnMode();
    }
}
