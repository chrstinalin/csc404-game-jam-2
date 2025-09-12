using System;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public abstract class PlayerMovementState {
    public abstract void EnterState(PlayerMovementManager manager);

    public abstract void UpdateState(PlayerMovementManager manager);
}
