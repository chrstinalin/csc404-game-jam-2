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
        outline = GetComponent<Outline>();
        movementManager = MovementManager.Instance;

        var interactableObject = GetComponent<InteractableObject>();
        if (interactableObject != null)
        {
            foreach (var character in interactableObject.characters)
            {
                if (character.CompareTag("MechPlayerEntity"))
                    isMechInteractable = true;

                if (character.CompareTag("MousePlayerEntity"))
                    isMouseInteractable = true;
            }
        }
    }

    private void Start()
    {
        movementManager = GameObject.FindGameObjectWithTag("MovementManager")?.GetComponent<MovementManager>();
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
        }
        else
        {
            outline.enabled = true;
        }
    }
}
