using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionTextManager : MonoBehaviour
{
    private Text textUI;
    private List<string> activeMessages = new List<string>();

    private void Awake()
    {
        textUI = GetComponent<Text>();
        UpdateDisplay();
    }

    public void AddMessage(string message)
    {
        if (!activeMessages.Contains(message))
        {
            activeMessages.Add(message);
            UpdateDisplay();
        }
    }

    public void RemoveMessage(string message)
    {
        if (activeMessages.Contains(message))
        {
            activeMessages.Remove(message);
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        if (activeMessages.Count > 0)
            textUI.text = activeMessages[0];
        else
            textUI.text = "";
    }
}
