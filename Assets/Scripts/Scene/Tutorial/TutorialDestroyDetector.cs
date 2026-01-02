using UnityEngine;

public class TutorialDestroyDetector : MonoBehaviour
{
    [Header("Tutorial Manager Reference")]
    public TutorialManager tutorialManager;

    [Header("Milestone to trigger on destruction")]
    public TutorialManager.Milestone milestone;

    private void OnDestroy()
    {
        if (tutorialManager != null)
        {
            tutorialManager.TriggerMilestone(milestone);
        }
    }
}
