using System;
using UnityEngine;

public class LockOnManager : MonoBehaviour
{
    [NonSerialized] public CameraManager CameraManager;
    [NonSerialized] public MovementManager MovementManager;

    public static bool lockOnMode = false;
    public event Action<bool> OnLockOnModeChanged;
    private bool _lastButtonState = false;

    private NavMeshEdgeVisualizer navMeshEdgeVisualizer;

    void Start()
    {
        CameraManager = CameraManager.Instance;
        MovementManager = MovementManager.Instance;

        var visualizer = new GameObject("NavMeshEdgeVisualizer");
        navMeshEdgeVisualizer = visualizer.AddComponent<NavMeshEdgeVisualizer>();

        OnLockOnModeChanged += HandleLockOnModeChanged;
    }

    void Update()
    {
        if (MovementManager == null)
        {
            if (lockOnMode)
            {
                lockOnMode = false;
                OnLockOnModeChanged?.Invoke(false);
                _lastButtonState = false;
            }
            return;
        }
        
        bool currentButtonState = Input.GetButton("ToggleLockOnMode");
        if (currentButtonState != _lastButtonState)
        {
            lockOnMode = currentButtonState;
            OnLockOnModeChanged?.Invoke(currentButtonState);
            _lastButtonState = currentButtonState;
        }
        
    }

    private void HandleLockOnModeChanged(bool isLocked)
    {
        MovementManager.isLockedMovement = isLocked;
        if (isLocked)
        {
            CameraManager.SetCameraFOV(Config.CAMERA_LOCK_ON_FOV);
            navMeshEdgeVisualizer.ShowFilledArea();
            MovementManager.Reset();
            MovementManager.MechAIController.Agent.enabled = true;
        }
        else
        {
            CameraManager.SetCameraFOV(Config.CAMERA_DEFAULT_FOV);
            navMeshEdgeVisualizer.ClearFilledArea();
        }
        ToggleEnemyOutlines(isLocked);
        PlayerMarker.Instance.setActive(isLocked);
    }

    private void ToggleEnemyOutlines(bool enable)
    {
        var enemies = GameObject.FindGameObjectsWithTag("EnemyBody");
        for (int i = 0; i < enemies.Length; i++)
        {
            var outline = enemies[i].GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = enable;
            }
        }
    }
}
