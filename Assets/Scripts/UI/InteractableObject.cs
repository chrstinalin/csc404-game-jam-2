using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class InteractableObject : MonoBehaviour
{
    [Header("Settings")]
    public string message = "Press E to interact";
    public GameObject[] characters;
    public GameObject textPrefab;
    private GameObject spawnedText;
    private bool isInside;

    private void OnTriggerEnter(Collider other)
    {
        foreach (var character in characters)
        {
            if (other.gameObject == character && spawnedText == null)
            {
                isInside = true;

                Canvas canvas = FindObjectOfType<Canvas>();
                spawnedText = Instantiate(textPrefab, canvas.transform);

                var textComponent = spawnedText.GetComponent<Text>();
                textComponent.text = message;

                var follower = spawnedText.GetComponent<InteractableObjectText>();
                follower.target = transform;

                break;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
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
}
