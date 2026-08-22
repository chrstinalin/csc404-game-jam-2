using UnityEngine;
using System;
using System.Collections.Generic;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
using TMPro;

public class LockOnManager : MonoBehaviour
{
    public static LockOnManager Instance;
    private CameraManager cameraManager;

    public event Action<GameObject> OnLockOnInteract;

    [SerializeField] private EventReference lockOnSFX;
    private EventInstance lockOnSFXInstance;
    [SerializeField] private LockOnUI LockOnUI;

    public Color lockonGreen;

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

    private Animator dkAnimator;

    [Header("Post-Processing")]
    [SerializeField] private Volume sceneVolume;
    private Volume runtimeVolume;
    private ColorAdjustments colorAdjustments;
    private Color defaultColorFilter;

    private Vignette vignette;
    private float defaultVignetteIntensity;

    private Coroutine fadeCoroutine;

    [Header("Target Action Text")]
    [SerializeField] private TextMeshProUGUI actionText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (sceneVolume != null)
        {
            runtimeVolume = gameObject.AddComponent<Volume>();
            runtimeVolume.isGlobal = true;
            runtimeVolume.priority = sceneVolume.priority;

            var clonedProfile = Instantiate(sceneVolume.profile);
            runtimeVolume.profile = clonedProfile;

            if (runtimeVolume.profile.TryGet<ColorAdjustments>(out var ca))
            {
                colorAdjustments = ca;
                defaultColorFilter = colorAdjustments.colorFilter.value;
            }

            if (runtimeVolume.profile.TryGet<Vignette>(out var vig))
            {
                vignette = vig;
                defaultVignetteIntensity = vignette.intensity.value;
            }
        }
    }

    private void Start()
    {
        if (dkAnimator == null && PlayerMech.Instance != null)
            dkAnimator = PlayerMech.Instance.GetComponentInChildren<Animator>();

        if (cameraManager == null && CameraManager.Instance != null)
            cameraManager = CameraManager.Instance;
    }

    void Update()
    {
        if (MovementManager.Instance.isLockedMovement)
        {
            isLockedOn = false;
            UpdateActionText();
            return;
        }
        else
        {
            isLockedOn = GameInput.LockOnMode && !MovementManager.Instance.IsMouseActive;
        }

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

        if (IsLockedOn && (GameInput.FireAxis > 0 || GameInput.InteractDown) && lockOnReset)
        {
            if (CurrentTarget != null)
            {
                OnLockOnInteract?.Invoke(CurrentTarget.gameObject);
                lockOnReset = false;
            }
        }

        if (GameInput.FireAxis == 0)
            lockOnReset = true;

        UpdateActionText();
    }

    private void UpdateActionText()
    {
        if (actionText == null) return;

        if (!isLockedOn || CurrentTarget == null || CurrentTarget.Type != LockOnObject.LockOnType.Enemy)
        {
            actionText.gameObject.SetActive(false);
            return;
        }

        actionText.gameObject.SetActive(true);
        actionText.text = IsCurrentTargetInRange() ? "<READY TO FIRE>" : "<OUT OF RANGE>";
    }

    private void EnterLockOn()
    {
        if (dkAnimator) dkAnimator.SetTrigger("isCharging");
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

        if (colorAdjustments != null)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeColorFilter(
                colorAdjustments.colorFilter.value,
                lockonGreen,
                0.05f
            ));
        }

        if (vignette != null)
        {
            vignette.intensity.Override(0.2f);
            vignette.color.Override(Color.black);
        }
    }

    private void ExitLockOn()
    {
        if (dkAnimator) dkAnimator.SetBool("isCharging", false);
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

        if (colorAdjustments != null)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(
                FadeColorFilter(colorAdjustments.colorFilter.value, defaultColorFilter, 0.5f)
            );
        }

        if (vignette != null)
        {
            vignette.intensity.Override(defaultVignetteIntensity);
        }
    }

    private IEnumerator FadeColorFilter(Color from, Color to, float duration)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            colorAdjustments.colorFilter.value = Color.Lerp(from, to, t);
            yield return null;
        }
        colorAdjustments.colorFilter.value = to;
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

        float horizontal = GameInput.TargetCycleAxis;

        if (GameInput.NextTargetDown)
            horizontal = 1f;
        else if (GameInput.PreviousTargetDown)
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

    public bool IsTargetInRange(LockOnObject target)
    {
        if (target == null) return false;

        if (target.Type != LockOnObject.LockOnType.Enemy)
            return true;

        float requiredDistance = target.GetLockOnRequiredDistance();
        Vector3 playerPos = PlayerMech.Instance.transform.position;

        // Aim at collider center if exists, fallback to centerPoint or transform
        Vector3 targetPos;
        Collider targetCollider = target.GetComponent<Collider>();
        if (targetCollider != null)
            targetPos = targetCollider.bounds.center;
        else if (target.centerPoint != null)
            targetPos = target.centerPoint.position;
        else
            targetPos = target.transform.position;

        float distance = Vector3.Distance(playerPos, targetPos);
        if (distance > requiredDistance)
            return false;

        Vector3 fireDirection = (targetPos - playerPos).normalized;
        RaycastHit hit;

        if (Physics.Raycast(playerPos, fireDirection, out hit, Mathf.Infinity))
        {
            bool hitTarget = hit.transform == target.transform 
                            || hit.transform.IsChildOf(target.transform) 
                            || target.transform.IsChildOf(hit.transform);

            return hitTarget;
        }

        return false;
    }
    public bool IsCurrentTargetInRange()
    {
        return IsTargetInRange(CurrentTarget);
    }

    public void ForceExitLockOn()
    {
        isLockedOn = false;
        ExitLockOn();
    }
}