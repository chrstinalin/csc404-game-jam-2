using System;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;

public class LockOnManager : MonoBehaviour
{
    [NonSerialized] public CameraManager CameraManager;
    [NonSerialized] public MovementManager MovementManager;
    [SerializeField] private EventReference lockOnSFX;

    public static bool lockOnMode = false;
    public event Action<bool> OnLockOnModeChanged;
    private bool _lastButtonState = false;

    private NavMeshEdgeVisualizer navMeshEdgeVisualizer;
    private EventInstance lockOnSFXInstance;

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
        bool playerControllingMech = MovementManager != null && !MovementManager.IsMouseActive;
        if (!playerControllingMech)
        {
            if (lockOnMode)
            {
                lockOnMode = false;
                OnLockOnModeChanged?.Invoke(false);
                _lastButtonState = false;
            }
            return;
        }

        bool buttonPressed = Input.GetButton("ToggleLockOnMode");
        float rtAxis = Input.GetAxisRaw("ToggleLockOnMode_RT");
        bool axisPressed = Mathf.Abs(rtAxis) >= Config.LOCK_ON_AXIS_THRESHOLD;
        bool currentButtonState = buttonPressed || axisPressed;

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
            if(BackgroundMusicManager.Instance) BackgroundMusicManager.Instance.LockOnMode(true);
            lockOnSFXInstance = AudioManager.Instance.PlaySFXWithParams(
                lockOnSFX,
                new Dictionary<string, float> { { "Activated", 1f } },
                transform.position,
                1f
            );

            CameraManager.SetCameraFOV(Config.CAMERA_LOCK_ON_FOV);
            navMeshEdgeVisualizer.ShowFilledArea();
            MovementManager.Reset();
        }
        else
        {
            if(BackgroundMusicManager.Instance) BackgroundMusicManager.Instance.LockOnMode(false);
            if (lockOnSFXInstance.isValid())
            {
                AudioManager.Instance.SetParameter(lockOnSFXInstance, "Activated", 0f);

                StartCoroutine(StopLockOnSFXAfterWrapUp(lockOnSFXInstance));
            }

            CameraManager.SetCameraFOV(Config.CAMERA_DEFAULT_FOV);
            navMeshEdgeVisualizer.ClearFilledArea();
        }

        ToggleEnemyOutlines(isLocked);
        PlayerMarker.Instance.setActive(isLocked);
    }

    private System.Collections.IEnumerator StopLockOnSFXAfterWrapUp(EventInstance instance)
    {
        yield return new WaitForSeconds(2f);

        if (instance.isValid())
        {
            AudioManager.Instance.StopSFX(instance);
        }
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
