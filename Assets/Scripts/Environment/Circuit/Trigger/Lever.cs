using UnityEngine;

public class Lever : TriggerAbstract
{
    [SerializeField] private GameObject offModel;
    [SerializeField] private GameObject onModel;

    [SerializeField] private bool startOn = false;
    private bool mouseInside = false;
    private MovementManager movementManager;

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
        if (movementManager.IsMouseActive && mouseInside && Input.GetButtonDown("Interact"))
        {
            ToggleLever();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == PlayerMouse.Instance.gameObject && movementManager.IsMouseActive)
        {
            mouseInside = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == PlayerMouse.Instance.gameObject && movementManager.IsMouseActive)
        {
            mouseInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == PlayerMouse.Instance.gameObject)
        {
            mouseInside = false;
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
        if(AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.LeverPullSFX, transform.position, 4f);
        if (IsActive) Deactivate();
        else Activate();
    }

    private void UpdateVisuals()
    {
        offModel.SetActive(!IsActive);
        onModel.SetActive(IsActive);
    }
}
