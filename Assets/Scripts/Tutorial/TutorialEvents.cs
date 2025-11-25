using UnityEngine;

public class TutorialEvents : MonoBehaviour
{
    [Header("Tutorial Manager")]
    public TutorialManager tutorialManager;

    private bool switchedToPeanutDone = false;

    public void CheckSwitchToPeanut()
    {
        if (switchedToPeanutDone)
            return;

        if (Input.GetButtonDown("MountKey"))
        {
            switchedToPeanutDone = true;

            tutorialManager.TriggerMilestone(TutorialManager.Milestone.SwitchedToPeanut);
        }
    }

    private void Update()
    {
        CheckSwitchToPeanut();
    }
}
