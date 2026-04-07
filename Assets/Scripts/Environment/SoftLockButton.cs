using UnityEngine;
using FMODUnity;

[RequireComponent(typeof(Collider))]
public class SoftlockButton : MonoBehaviour
{
    [SerializeField] private Lever targetLever;
    [SerializeField] private GameObject unpressedModel;
    [SerializeField] private GameObject pressedModel;
    [SerializeField] private EventReference buttonPressSFX;

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
        PressButton();
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
            AudioManager.Instance.PlaySFX(buttonPressSFX, transform.position, 5f);
        }
    }

    private void UpdateVisuals(bool pressed)
    {
        unpressedModel.SetActive(!pressed);
        pressedModel.SetActive(pressed);
    }
}
