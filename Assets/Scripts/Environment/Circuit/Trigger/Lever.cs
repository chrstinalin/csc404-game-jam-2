using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Lever : TriggerAbstract
{
    [SerializeField] private GameObject offModel;
    [SerializeField] private GameObject onModel;
    private int count;

    [SerializeField] private bool startOn = false;
    private bool mouseInside = false;
    private MovementManager movementManager;

    private Animator mouseAnimator; // cache animator

    private void Start()
    {
        movementManager = MovementManager.Instance;
    }

    private void Awake()
    {
        if (!offModel || !onModel)
            throw new MissingReferenceException($"{name}: Both off and on models must be assigned.");

        IsActive = startOn;
        UpdateVisuals();
    }

    private void Update()
    {
        var mouse = PlayerMouse.Instance;

        // Detect losing mouse
        if (mouseInside)
        {
            if (mouse == null || !mouse.gameObject.activeInHierarchy)
            {
                CancelMouseInteraction();
                return;
            }
        }

        if (movementManager.IsMouseActive && mouseInside && mouse != null)
        {
            if (Input.GetButtonDown("Interact"))
            {
                mouseAnimator = mouse.GetComponentInChildren<Animator>();

                if (mouseAnimator != null)
                {
                    Debug.Log("playing" + count);
                    count = count + 1;
                    mouseAnimator.SetTrigger("Interact");

                }

                ToggleLever();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (PlayerMouse.Instance != null &&
            other.gameObject == PlayerMouse.Instance.gameObject &&
            movementManager.IsMouseActive)
        {
            mouseInside = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (PlayerMouse.Instance != null &&
            other.gameObject == PlayerMouse.Instance.gameObject &&
            movementManager.IsMouseActive)
        {
            mouseInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (PlayerMouse.Instance != null &&
            other.gameObject == PlayerMouse.Instance.gameObject)
        {
            CancelMouseInteraction();
        }
    }

    private void CancelMouseInteraction()
    {
        mouseInside = false;

        if (mouseAnimator != null)
        {
            mouseAnimator.ResetTrigger("Interact");
        }
    }

    public override void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        UpdateVisuals();
    }

    public override void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        UpdateVisuals();
    }

    public void ToggleLever()
    {
        if(AudioManager.Instance != null) 
            AudioManager.Instance.PlaySFX(AudioManager.Instance.LeverPullSFX, transform.position, 4f);

        if (IsActive) Deactivate();
        else Activate();
    }

    private void UpdateVisuals()
    {
        offModel.SetActive(!IsActive);
        onModel.SetActive(IsActive);
    }
}