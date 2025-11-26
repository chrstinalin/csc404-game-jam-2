using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class InteractableObject : MonoBehaviour
{
    [Header("Settings")]
    public string actionMessage = "interact";
    public GameObject[] characters = new GameObject[0];
    public GameObject textPrefab;
    private GameObject spawnedText;
    private bool isInside;

    private void OnTriggerEnter(Collider other)
    {
        if (characters == null || characters.Length == 0)
        {
            return;
        }
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
                follower.target = transform;

                break;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (characters == null || characters.Length == 0)
        {
            return;
        }
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
        string button = ButtonMappings.GetButtonLabel(action);
        return $"Press {button} to {actionMessage}";
    }

}
