using UnityEngine;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    public TutorialText tutorialText;

    [Header("Required Milestones For This Tutorial (in order)")]
    public List<Milestone> requiredMilestones = new List<Milestone>();

    public enum Milestone
    {
        SwitchedToPeanut,
        LeverPulled,
        RoomExited,
        WalkedAroundLasers,
        ActivateLockOnMode,
        DestroyEnemy
    }

    private int nextRequiredIndex = 0;

    public void TriggerMilestone(Milestone milestone)
    {
        if (nextRequiredIndex >= requiredMilestones.Count)
            return;

        if (requiredMilestones[nextRequiredIndex] != milestone)
            return;

        nextRequiredIndex++;

        tutorialText.Next();
    }

    public bool IsCompleted(Milestone milestone)
    {
        int index = requiredMilestones.IndexOf(milestone);
        return index >= 0 && index < nextRequiredIndex;
    }
}
