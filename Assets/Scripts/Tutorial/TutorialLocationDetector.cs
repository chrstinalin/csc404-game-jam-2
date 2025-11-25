using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ExitRoomTrigger : MonoBehaviour
{
    [Header("Tutorial Manager")]
    public TutorialManager tutorialManager;
    
    [Header("Milestone to Trigger")]
    public TutorialManager.Milestone milestone;
    private bool milestoneTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (milestoneTriggered) return;

        if (other.gameObject == PlayerMouse.Instance.gameObject || 
            other.gameObject == PlayerMech.Instance.gameObject)
        {
            milestoneTriggered = true;
            tutorialManager.TriggerMilestone(milestone);
        }
    }
}
