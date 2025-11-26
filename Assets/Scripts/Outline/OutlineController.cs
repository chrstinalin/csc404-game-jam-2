using UnityEngine;

public class OutlineController : MonoBehaviour
{
    private Outline outline;
    public bool isMechInteractable;
    public bool isMouseInteractable;

    public MovementManager movementManager;

    private bool lastMouseState;

    private void Awake()
    {
        movementManager = MovementManager.Instance;

        var interactableObject = GetComponent<InteractableObject>();
        if (interactableObject != null)
        {
            foreach (var character in interactableObject.characters)
            {   
                Debug.Log(interactableObject.characters);
                if (character.CompareTag("MechPlayerEntity"))
                    isMechInteractable = true;

                if (character.CompareTag("MousePlayerEntity"))
                    isMouseInteractable = true;
            }
        }
    }

    private void Start()
    {
        movementManager = MovementManager.Instance;
        outline = GetComponentInChildren<Outline>();
        lastMouseState = movementManager.IsMouseActive;
        UpdateOutline();
    }

    private void Update()
    {
        if (movementManager.IsMouseActive != lastMouseState)
        {
            lastMouseState = movementManager.IsMouseActive;
            UpdateOutline();
        }
    }

    public void UpdateOutline()
    {
        bool mouseActive = movementManager.IsMouseActive;

        bool shouldClear =
            (isMechInteractable && mouseActive) ||
            (isMouseInteractable && !mouseActive);

        if (shouldClear)
        {
            outline.enabled = false;
            Debug.Log("cleared");
        }
        else
        {
            outline.enabled = true;
            Debug.Log("enabled");
        }
    }
}
