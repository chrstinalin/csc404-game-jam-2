using UnityEngine;
using System;

public abstract class TriggerableAbstract : MonoBehaviour
{
    public bool IsOn { get; protected set; }

    public abstract void TurnOn();
    public abstract void TurnOff();
}
