using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SoftlockButton : MonoBehaviour
{
    [SerializeField] private Lever targetLever;
    [SerializeField] private GameObject unpressedModel;
    [SerializeField] private GameObject pressedModel;

    private void Awake()
    {
        if (!targetLever)
            throw new MissingReferenceException($"{name}: Target lever must be assigned.");
        if (!unpressedModel || !pressedModel)
            throw new MissingReferenceException($"{name}: Both unpressed and pressed models must be assigned.");

        GetComponent<Collider>().isTrigger = true;
        UpdateVisuals(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == PlayerMouse.Instance.gameObject)
        {
            PressButton();
        }
    }

    private void PressButton()
    {
        if (targetLever.IsActive)
        {
            targetLever.Deactivate();
        }

        UpdateVisuals(true);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonPressSFX, transform.position, 5f);
        }
    }

    private void UpdateVisuals(bool pressed)
    {
        unpressedModel.SetActive(!pressed);
        pressedModel.SetActive(pressed);
    }
}
