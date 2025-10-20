using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractableObjectMessage : MonoBehaviour
{
    public string message = "Press E to interact";
    public GameObject[] characters;
    public InteractionTextManager textManager;

    private bool isInside = false;

    private void OnTriggerEnter(Collider other)
    {
        foreach (var character in characters)
        {
            if (other.gameObject == character)
            {
                isInside = true;
                textManager.AddMessage(message);
                break;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        foreach (var character in characters)
        {
            if (other.gameObject == character)
            {
                isInside = false;
                textManager.RemoveMessage(message);
                break;
            }
        }
    }

    private void OnDisable()
    {
        if (isInside && textManager != null)
            textManager.RemoveMessage(message);
    }
}
