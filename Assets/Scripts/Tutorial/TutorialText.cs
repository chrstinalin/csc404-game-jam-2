using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TutorialText : MonoBehaviour
{
    [Header("UI")]
    public Text textUI;

    [Header("Text List")]
    [TextArea]
    public string[] lines;
    private int index = 0;

    [Header("Typing Settings")]
    public float typeSpeed = 0.02f;

    private Coroutine typingRoutine;

    void Start()
    {
        ShowCurrentLine();
    }

    public void Next()
    {
        if (index < lines.Length - 1)
        {
            index++;
            ShowCurrentLine();
        }
    }

    private void ShowCurrentLine()
    {
        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        string line = lines[index];
        typingRoutine = StartCoroutine(TypeText(line));
    }

    private IEnumerator TypeText(string line)
    {
        textUI.text = "";

        string mainText = line;
        string directiveText = "";
        int directiveIndex = line.IndexOf("[DIRECTIVE]");

        if (directiveIndex >= 0)
        {
            mainText = line.Substring(0, directiveIndex);
            directiveText = line.Substring(directiveIndex);
        }

        foreach (char c in mainText)
        {
            textUI.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        if (!string.IsNullOrEmpty(directiveText))
        {
            string colored = "<color=#fbbf2b>" + directiveText + "</color>";
            string current = "<color=#fbbf2b>";

            foreach (char c in directiveText)
            {
                current += c;
                textUI.text += c;
                yield return new WaitForSeconds(typeSpeed);
            }

            textUI.text = mainText + colored;
        }
    }
}
