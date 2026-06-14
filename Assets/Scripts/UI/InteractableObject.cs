using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class InteractableObject : MonoBehaviour
{
    [Header("Settings")]
    public string actionMessage = "interact";
    public GameObject[] characters = new GameObject[0];
    public GameObject textPrefab;

    [Header("Text Settings")]
    public string promptPrefix = "Press";

    [Header("Optional Override")]
    public Transform textPositionOverride;

    private GameObject spawnedText;
    private bool isInside;

    private void Update()
    {
        if (isInside)
        {
            RefreshInteractionState();
        }
    }
    public void ForceRefresh()
    {
        RefreshInteractionState();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (IsTrackedCharacter(other.gameObject))
        {
            isInside = true;
            RefreshInteractionState();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (IsTrackedCharacter(other.gameObject))
        {
            isInside = true;
            RefreshInteractionState();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsTrackedCharacter(other.gameObject))
        {
            isInside = false;

            if (spawnedText != null)
            {
                Destroy(spawnedText);
                spawnedText = null;
            }
        }
    }

    private bool IsTrackedCharacter(GameObject obj)
    {
        if (characters == null || characters.Length == 0)
            return false;

        foreach (var character in characters)
        {
            if (character == obj)
                return true;
        }

        return false;
    }

    private bool IsCorrectActiveCharacter(GameObject character)
    {
        bool isMouseActive = MovementManager.Instance.IsMouseActive;

        if (character.name.ToLower().Contains("mouse"))
            return isMouseActive;

        if (character.name.ToLower().Contains("mech"))
            return !isMouseActive;

        return false;
    }

    private void RefreshInteractionState()
    {
        bool correctCharacterInside = false;

        foreach (var character in characters)
        {
            if (character != null && character.activeInHierarchy)
            {
                if (IsCorrectActiveCharacter(character))
                {
                    correctCharacterInside = true;
                    break;
                }
            }
        }

        if (!correctCharacterInside)
        {
            if (spawnedText != null)
            {
                Destroy(spawnedText);
                spawnedText = null;
            }
            return;
        }

        if (spawnedText == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            spawnedText = Instantiate(textPrefab, canvas.transform);
            spawnedText.transform.SetAsFirstSibling();

            var textComponent = spawnedText.GetComponent<Text>();
            textComponent.text = GetMessage(ActionType.Interact);

            var follower = spawnedText.GetComponent<InteractableObjectText>();
            follower.target = textPositionOverride != null ? textPositionOverride : transform;
        }
        else
        {
            var textComponent = spawnedText.GetComponent<Text>();
            textComponent.text = GetMessage(ActionType.Interact);
        }
    }

    private string GetMessage(ActionType action)
    {
        if (string.IsNullOrEmpty(actionMessage))
            return "";

        string button = ButtonMappings.GetButtonLabel(action);
        return $"{promptPrefix} {button} to {actionMessage}";
    }

    public void ChangeText(string newMessage)
    {
        actionMessage = newMessage;

        if (spawnedText != null)
        {
            var textComponent = spawnedText.GetComponent<Text>();
            textComponent.text = GetMessage(ActionType.Interact);
        }
    }
}