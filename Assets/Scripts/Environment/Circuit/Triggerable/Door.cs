using UnityEngine;

public class Door : TriggerableAbstract
{
    [SerializeField] private GameObject closedDoor;
    [SerializeField] private GameObject openDoor;

    private void Awake()
    {
        if (!closedDoor || !openDoor)
            throw new MissingReferenceException(
                $"{name}: Both closed and open door models must be assigned.");

        IsOn = false;
        closedDoor.SetActive(!IsOn);
        openDoor.SetActive(IsOn);
    }

    public override void TurnOn()
    {
        if (IsOn) return;

        IsOn = true;
        closedDoor.SetActive(!IsOn);
        openDoor.SetActive(IsOn);

        InvokeTurnedOn();
    }

    public override void TurnOff()
    {
        if (!IsOn) return;

        IsOn = false;
        closedDoor.SetActive(!IsOn);
        openDoor.SetActive(IsOn);

        InvokeTurnedOff();
    }
}
