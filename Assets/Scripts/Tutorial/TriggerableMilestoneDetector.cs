using UnityEngine;

public class TriggerableMilestoneDetector : MonoBehaviour
{
    [Header("Tutorial Manager")]
    public TutorialManager tutorialManager;

    [Header("Triggerable Object")]
    public TriggerAbstract targetTrigger;

    [Header("Milestone to Trigger")]
    public TutorialManager.Milestone milestone;

    private bool milestoneTriggered = false;

    private void Update()
    {
        if (milestoneTriggered || targetTrigger == null) return;

        if (targetTrigger.IsActive)
        {
            milestoneTriggered = true;
            tutorialManager.TriggerMilestone(milestone);
        }
    }
}
