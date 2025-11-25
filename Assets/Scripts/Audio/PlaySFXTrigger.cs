using UnityEngine;
using FMODUnity;

public class PlaySFXTrigger : MonoBehaviour
{
    [SerializeField] private TriggerAbstract trigger;
    [SerializeField] private EventReference sfxEvent;

    private bool lastState;

    private void Awake()
    {
        if (trigger == null)
            trigger = GetComponent<TriggerAbstract>();

        if (trigger != null)
            lastState = trigger.IsActive;
    }

    private void Update()
    {
        if (trigger == null)
            return;

        if (!lastState && trigger.IsActive)
        {
            AudioManager.Instance.PlaySFX(sfxEvent, transform.position, 1f);
        }

        lastState = trigger.IsActive;
    }
}
