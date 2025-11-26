using UnityEngine;

public class OutlineController : MonoBehaviour
{
    private Outline outline;
    public bool isMechInteractable;
    public bool isMouseInteractable;

    private MovementManager movementManager;

    private bool lastMouseState;
    private bool lastLockOnState;

    private void Awake()
    {
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
        movementManager = MovementManager.Instance;
        outline = GetComponentInChildren<Outline>();

        lastMouseState = movementManager.IsMouseActive;
        lastLockOnState = LockOnManager.lockOnMode;

        UpdateOutline();
    }

    private void Update()
    {
        bool currentMouseState = movementManager.IsMouseActive;
        bool currentLockState = LockOnManager.lockOnMode;

        // Only update when something changes
        if (currentMouseState != lastMouseState || currentLockState != lastLockOnState)
        {
            lastMouseState = currentMouseState;
            lastLockOnState = currentLockState;

            UpdateOutline();
        }
    }

    public void UpdateOutline()
    {
        bool mouseActive = movementManager.IsMouseActive;
        bool lockedOn = LockOnManager.lockOnMode;

        if (lockedOn)
        {
            outline.enabled = false;
            return;
        }

        bool shouldClear =
            (isMechInteractable && mouseActive) ||
            (isMouseInteractable && !mouseActive);

        outline.enabled = !shouldClear;
    }
}
