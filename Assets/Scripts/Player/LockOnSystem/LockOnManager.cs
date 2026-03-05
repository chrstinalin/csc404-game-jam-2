using UnityEngine;
using System;
using System.Collections.Generic;
using FMODUnity;
using FMOD.Studio;

public class LockOnManager : MonoBehaviour
{
    public static LockOnManager Instance;
    private CameraManager cameraManager;

    public event Action<GameObject> OnLockOnInteract;

    [SerializeField] private EventReference lockOnSFX;
    private EventInstance lockOnSFXInstance;
    [SerializeField] private LockOnUI LockOnUI;

    private bool isLockedOn;
    private bool sfxPlaying;
    private bool targetsInitialized = false;

    private LockOnObject[] visibleTargets = new LockOnObject[0];
    private int currentTargetIndex = 0;

    private float cycleThreshold = 0.5f;
    private float lastHorizontalInput = 0f;
    private bool lockOnReset;

    public bool IsLockedOn => isLockedOn;

    public LockOnObject CurrentTarget => GetCurrentTarget();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        cameraManager = CameraManager.Instance;
    }

    void Update()
    {
        isLockedOn = Input.GetButton("ToggleLockOnMode") && !MovementManager.Instance.IsMouseActive;

        if (cameraManager != null)
            cameraManager.SetLockOn(isLockedOn);

        if (isLockedOn && !MovementManager.Instance.IsMouseActive)
        {
            EnterLockOn();
            HandleTargetCycling();
        }
        else
        {
            ExitLockOn();
        }

        if (IsLockedOn && (Input.GetAxis("LockOn") > 0 || Input.GetButtonDown("Interact")) && lockOnReset)
        {
            if (CurrentTarget != null)
            {
                OnLockOnInteract?.Invoke(CurrentTarget.gameObject);
                lockOnReset = false;
            }
        }

        if (Input.GetAxis("LockOn") == 0)
        {
            lockOnReset = true;
        }
    }

    private void EnterLockOn()
    {
        if (sfxPlaying) return;

        lockOnSFXInstance = RuntimeManager.CreateInstance(lockOnSFX);
        lockOnSFXInstance.setParameterByName("Activated", 1f);
        lockOnSFXInstance.start();

        if (BackgroundMusicManager.Instance)
            BackgroundMusicManager.Instance.LockOnMode(true);

        sfxPlaying = true;

        if (!targetsInitialized)
        {
            InitializeTargets();
            targetsInitialized = true;
        }
    }

    private void ExitLockOn()
    {
        if (!sfxPlaying) return;

        if (lockOnSFXInstance.isValid())
            lockOnSFXInstance.setParameterByName("Activated", 0f);

        if (BackgroundMusicManager.Instance)
            BackgroundMusicManager.Instance.LockOnMode(false);

        sfxPlaying = false;
        visibleTargets = new LockOnObject[0];
        currentTargetIndex = 0;
        lastHorizontalInput = 0f;
        targetsInitialized = false;

        if (LockOnUI != null)
            LockOnUI.UpdateUI(null);

        if (MechAIController.Instance != null)
            MechAIController.Instance.SetTarget(null);
    }

    private void InitializeTargets()
    {
        LockOnObject[] lockables = FindObjectsOfType<LockOnObject>();
        List<LockOnObject> tempList = new List<LockOnObject>();

        foreach (var obj in lockables)
        {
            Vector3 vp = cameraManager.Cam.WorldToViewportPoint(obj.transform.position);
            if (vp.z > 0f && vp.x > 0f && vp.x < 1f && vp.y > 0f && vp.y < 1f)
                tempList.Add(obj);
        }

        tempList.Sort((a, b) =>
        {
            float ax = cameraManager.Cam.WorldToViewportPoint(a.transform.position).x;
            float bx = cameraManager.Cam.WorldToViewportPoint(b.transform.position).x;
            return ax.CompareTo(bx);
        });

        visibleTargets = tempList.ToArray();
        currentTargetIndex = 0;
    }

    private void HandleTargetCycling()
    {
        for (int i = visibleTargets.Length - 1; i >= 0; i--)
        {
            if (visibleTargets[i] == null)
            {
                List<LockOnObject> temp = new List<LockOnObject>(visibleTargets);
                temp.RemoveAt(i);
                visibleTargets = temp.ToArray();

                if (currentTargetIndex >= visibleTargets.Length)
                    currentTargetIndex = visibleTargets.Length - 1;
            }
        }

        if (visibleTargets.Length == 0)
        {
            currentTargetIndex = 0;

            if (MechAIController.Instance != null)
                MechAIController.Instance.SetTarget(null);

            if (LockOnUI != null)
                LockOnUI.UpdateUI(null);

            return;
        }

        // Cycling for both controller and keyboard/mouse
        float horizontal = Input.GetAxis("HorizontalRightJoystick");

        if (Input.GetButtonDown("NextTarget"))
            horizontal = 1f;
        else if (Input.GetButtonDown("PreviousTarget"))
            horizontal = -1f;

        if (horizontal > cycleThreshold && lastHorizontalInput <= cycleThreshold)
        {
            if (currentTargetIndex < visibleTargets.Length - 1)
                currentTargetIndex++;
        }
        else if (horizontal < -cycleThreshold && lastHorizontalInput >= -cycleThreshold)
        {
            if (currentTargetIndex > 0)
                currentTargetIndex--;
        }

        lastHorizontalInput = horizontal;

        if (MechAIController.Instance != null && CurrentTarget != null)
            MechAIController.Instance.SetTarget(CurrentTarget.gameObject);

        if (LockOnUI != null)
            LockOnUI.UpdateUI(CurrentTarget);
    }

    public LockOnObject GetCurrentTarget()
    {
        if (visibleTargets == null || visibleTargets.Length == 0)
            return null;

        if (currentTargetIndex < 0 || currentTargetIndex >= visibleTargets.Length)
            return null;

        return visibleTargets[currentTargetIndex];
    }
}