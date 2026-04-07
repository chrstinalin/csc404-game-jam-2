using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class InteractableObject : MonoBehaviour
{
    [Header("Settings")]
    public string actionMessage = "interact";
    public GameObject[] characters = new GameObject[0];
    public GameObject textPrefab;
    private MovementManager movementManager;

    [Header("Text Settings")]
    public string promptPrefix = "Press";

    [Header("Optional Override")]
    public Transform textPositionOverride;

    private GameObject spawnedText;
    private bool isInside;

    private void Update()
    {
        if (isInside && spawnedText != null)
        {
            bool shouldHide = false;

            foreach (var character in characters)
            {
                if (character == null || !character.activeInHierarchy)
                {
                    shouldHide = true;
                    break;
                }
            }

            if (shouldHide)
            {
                Destroy(spawnedText);
                spawnedText = null;
                isInside = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (characters == null || characters.Length == 0)
            return;

        foreach (var character in characters)
        {
            if (other.gameObject == character && spawnedText == null)
            {
                isInside = true;

                Canvas canvas = FindObjectOfType<Canvas>();
                spawnedText = Instantiate(textPrefab, canvas.transform);

                var textComponent = spawnedText.GetComponent<Text>();
                textComponent.text = GetMessage(ActionType.Interact);

                var follower = spawnedText.GetComponent<InteractableObjectText>();
                follower.target = textPositionOverride != null ? textPositionOverride : transform;

                break;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (characters == null || characters.Length == 0)
            return;

        foreach (var character in characters)
        {
            if (other.gameObject == character && spawnedText != null)
            {
                isInside = false;
                Destroy(spawnedText);
                spawnedText = null;
                break;
            }
        }
    }

    private void OnDisable()
    {
        if (spawnedText != null)
        {
            Destroy(spawnedText);
            spawnedText = null;
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