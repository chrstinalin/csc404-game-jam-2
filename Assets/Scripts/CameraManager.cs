using System;
using UnityEngine;

public class CameraManager : CameraMovementManager
{
    public static CameraManager Instance;

    public GameObject FollowEntity;
    [NonSerialized] public Transform CameraPivot;
    [NonSerialized] public Camera Cam;

    [Header("Collision")]
    [SerializeField] private LayerMask wallLayer;

    private float heightOffset;
    private float targetFOV;
    private float fovVelocity;

    private float yaw;
    private float pitch;
    private float zoom = 5f;
    private float maxZoom = Config.CAMERA_MAX_ZOOM;

    private float currentCameraDistance;
    private float collisionDistanceVelocity;
    private float targetCameraDistance;
    private float targetDistanceVelocity;

    public bool IsLockedOn { get; private set; }

    // The settings slider, as a percentage: 100 is the default, 200 the cap.
    // Scales mouse and stick alike so one slider covers both.
    public float lookSensitivity;
    public bool invertYAxis;
    private float lockOnFOVMultiplier = 0.90f;

    private Vector3 lockedPosition;
    private Quaternion lockedRotation;
    private bool isCameraLocked = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Cam = GetComponent<Camera>();

        CameraPivot = transform.parent;
        if (CameraPivot == null)
            CameraPivot = new GameObject("CameraPivot").transform;

        heightOffset = CameraPivot.position.y;
        targetFOV = Config.CAMERA_DEFAULT_FOV;

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        currentCameraDistance = zoom;
        targetCameraDistance = zoom;

        Cursor.lockState = CursorLockMode.Locked;

        lookSensitivity = PlayerPrefs.GetInt("MouseSensitivityMultiplier", Config.SENSITIVITY_MULTIPLIER_DEFAULT) * 10;
        invertYAxis = PlayerPrefs.GetInt("InvertYAxis", 0) == 1;
    }

    void Update()
    {
        if (Cam != null)
        {
            Cam.fieldOfView = Mathf.SmoothDamp(
                Cam.fieldOfView,
                targetFOV,
                ref fovVelocity,
                Config.CAMERA_SMOOTH_TIME
            );
        }
    }

    public void SetLockOn(bool locked)
    {
        IsLockedOn = locked;

        targetFOV = locked
            ? Config.CAMERA_DEFAULT_FOV * lockOnFOVMultiplier
            : Config.CAMERA_DEFAULT_FOV;
    }

    public override void SetFollowEntity(GameObject? entity, float? newMaxZoom = null)
    {
        FollowEntity = entity;

        if (newMaxZoom.HasValue)
        {
            maxZoom = newMaxZoom.Value;

            if (zoom > maxZoom)
                zoom = maxZoom;
        }
    }

    public override void UpdateCamera()
    {   
        if (FollowEntity == null)
        return;

        if (IsLockedOn)
            return;

        if (isCameraLocked)  // Add this
            return;
        if (FollowEntity == null)
            return;

        if (IsLockedOn)
            return;

        // Follow target
        CameraPivot.position = new Vector3(
            FollowEntity.transform.position.x,
            FollowEntity.transform.position.y + heightOffset,
            FollowEntity.transform.position.z
        );

        HandleZoom();
        HandleRotation();

        // Rotation
        Quaternion targetRotation = Quaternion.Euler(pitch, yaw, 0f);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Config.SMOOTH_TIME
        );

        float desiredDistance = zoom;

        float collisionDistance = GetCollisionDistance(
            CameraPivot.position,
            transform.forward,
            desiredDistance
        );

        if (collisionDistance < currentCameraDistance)
        {
            currentCameraDistance = collisionDistance;
            targetCameraDistance = collisionDistance;
        }
        else
        {
            targetCameraDistance = Mathf.SmoothDamp(
                targetCameraDistance,
                collisionDistance,
                ref targetDistanceVelocity,
                Config.CAMERA_COLLISION_EASE_TIME
            );

            currentCameraDistance = Mathf.SmoothDamp(
                currentCameraDistance,
                targetCameraDistance,
                ref collisionDistanceVelocity,
                Config.CAMERA_COLLISION_SMOOTH_TIME
            );
        }

        // Final position
        transform.position = CameraPivot.position - transform.forward * currentCameraDistance;
    }

    private float GetCollisionDistance(Vector3 pivotPos, Vector3 cameraForward, float desiredDistance)
    {
        Vector3 dir = -cameraForward;
        float minDistance = Config.CAMERA_MIN_DISTANCE;

        RaycastHit hit;

        if (Physics.SphereCast(
            pivotPos,
            Config.CAMERA_COLLISION_RADIUS,
            dir,
            out hit,
            desiredDistance,
            wallLayer,
            QueryTriggerInteraction.Ignore))
        {
            float safeDistance = hit.distance - Config.CAMERA_COLLISION_BUFFER;
            return Mathf.Clamp(safeDistance, minDistance, desiredDistance);
        }

        return desiredDistance;
    }

    private void HandleZoom()
    {
        if (GameInput.ZoomModifier)
        {
            zoom -= GameInput.ZoomAxis * Config.ZOOM_SENSITIVITY * Time.deltaTime;
        }
        else
        {
            zoom -= GameInput.ScrollAxis * Config.ZOOM_SENSITIVITY;
        }

        zoom = Mathf.Clamp(zoom, Config.CAMERA_MIN_ZOOM, maxZoom);
    }

    private void HandleRotation()
    {
        // One slider drives both devices, but they are different quantities:
        // the mouse hands us a displacement that is already complete, the stick
        // a rate that has to be spread over the frame. Only the stick takes
        // deltaTime - applying it to the mouse as well is what left mouse look
        // so slow, and made it slower still at higher framerates.
        float scale = lookSensitivity / 100f;

        Vector2 look = GameInput.LookDelta * (Config.MOUSE_LOOK_SENSITIVITY * scale)
                     + GameInput.LookStick * (Config.STICK_LOOK_SENSITIVITY * scale * Time.deltaTime);

        yaw += look.x;
        pitch -= look.y * (invertYAxis ? -1 : 1);

        pitch = Mathf.Clamp(pitch, Config.MIN_PITCH, Config.MAX_PITCH);
    }

    public void SetCameraLock(Vector3 position, bool isLocked, Quaternion rotation = new Quaternion())
    {
        isCameraLocked = isLocked;

        if (isLocked)
        {
            lockedPosition = position;
            lockedRotation = rotation;
            CameraPivot.position = position;
            CameraPivot.rotation = rotation;
            currentCameraDistance = 0f;
            targetCameraDistance = 0f;
            transform.localPosition = Vector3.zero;  
            transform.localRotation = Quaternion.identity;
        }
    }

    private void LateUpdate()
    {
        if (isCameraLocked)
        {
            CameraPivot.position = lockedPosition;
            CameraPivot.rotation = lockedRotation;
            currentCameraDistance = 0f;
            targetCameraDistance = 0f;
        }
    }   

    public void ForceUnlockCamera()
    {
        isCameraLocked = false;
    }

    public override void PanTo(float zoomSize) => zoom = zoomSize;
    public override void SetMaxZoom(float max) => maxZoom = max;
    public void SetCameraFOV(float newFOV) => targetFOV = newFOV;
}