using UnityEngine;
using System;

public abstract class TriggerableAbstract : MonoBehaviour
{
    public bool IsOn { get; protected set; }

    public event Action OnTurnedOn;
    public event Action OnTurnedOff;

    public abstract void TurnOn();
    public abstract void TurnOff();

    protected void InvokeTurnedOn() => OnTurnedOn?.Invoke();
    protected void InvokeTurnedOff() => OnTurnedOff?.Invoke();
}
