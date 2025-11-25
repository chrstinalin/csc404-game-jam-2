using UnityEngine;
using FMODUnity;

public class PlaySFXTriggerable : MonoBehaviour
{
    [SerializeField] private TriggerableAbstract triggerable;
    [SerializeField] private EventReference sfxEvent;

    private void Awake()
    {
        if (triggerable == null)
            triggerable = GetComponent<TriggerableAbstract>();

        if (triggerable != null)
            triggerable.OnTurnedOn += PlaySFX;
    }

    private void OnDestroy()
    {
        if (triggerable != null)
            triggerable.OnTurnedOn -= PlaySFX;
    }

    private void PlaySFX()
    {
        AudioManager.Instance.PlaySFX(sfxEvent);
    }
}
