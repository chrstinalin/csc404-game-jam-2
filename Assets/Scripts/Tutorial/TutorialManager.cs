using UnityEngine;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    public TutorialText tutorialText;

    public enum Milestone
    {
        SwitchedToPeanut,
        LeverPulled,
        RoomExited,

    }

    private HashSet<Milestone> completed = new HashSet<Milestone>();

    public void TriggerMilestone(Milestone milestone)
    {
        if (completed.Contains(milestone))
            return;

        completed.Add(milestone);

        Debug.Log("Milestone completed: " + milestone);

        tutorialText.Next();
    }

    public bool IsCompleted(Milestone milestone)
    {
        return completed.Contains(milestone);
    }
}
