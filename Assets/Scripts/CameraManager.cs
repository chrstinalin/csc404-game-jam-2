using System;
using UnityEngine;

public class CameraManager : CameraMovementManager
{
    public static CameraManager Instance;

    public GameObject FollowEntity;
    [NonSerialized] public Transform CameraPivot;
    [NonSerialized] public Camera Cam;

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

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Cam = GetComponent<Camera>();
        CameraPivot = transform.parent; // Make sure the camera has a parent
        if (CameraPivot == null)
            CameraPivot = new GameObject("CameraPivot").transform;

        heightOffset = CameraPivot.position.y;
        targetFOV = Config.CAMERA_DEFAULT_FOV;

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        currentCameraDistance = zoom;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Smooth FOV transition
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

    public override void SetFollowEntity(GameObject? entity, float? newMaxZoom = null)
    {
        FollowEntity = entity;
        if (newMaxZoom.HasValue)
        {
            maxZoom = newMaxZoom.Value;
            if (zoom > maxZoom)
            {
                zoom = maxZoom;
            }
        }
    }

    public override void UpdateCamera()
    {
        if (FollowEntity == null)
            return;

        // Follow the target entity
        CameraPivot.position = new Vector3(
            FollowEntity.transform.position.x,
            FollowEntity.transform.position.y + heightOffset,
            FollowEntity.transform.position.z
        );

        HandleZoom();
        HandleRotation();

        // Apply rotation
        Quaternion targetRotation = Quaternion.Euler(pitch, yaw, 0f);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Config.SMOOTH_TIME
        );

        float desiredDistance = zoom;

        float instantCollisionDistance = GetCollisionAdjustedDistanceOnlyWalls(
            CameraPivot.position,
            transform.forward,
            desiredDistance
        );
        
        targetCameraDistance = Mathf.SmoothDamp(
            targetCameraDistance,
            instantCollisionDistance,
            ref targetDistanceVelocity,
            Config.CAMERA_COLLISION_EASE_TIME
        );

        currentCameraDistance = Mathf.SmoothDamp(
            currentCameraDistance,
            targetCameraDistance,
            ref collisionDistanceVelocity,
            Config.CAMERA_COLLISION_SMOOTH_TIME
        );

        // Place camera
        transform.position = CameraPivot.position - transform.forward * currentCameraDistance;
    }

    private float GetCollisionAdjustedDistanceOnlyWalls(Vector3 pivotPos, Vector3 cameraForward, float desiredDistance)
    {
        Vector3 dirToCamera = -cameraForward;

        float charOffset = 0f;
        Transform playerRoot = FollowEntity != null ? FollowEntity.transform : null;
        float minDistanceFromChar = Config.CAMERA_MIN_DISTANCE;
        if (playerRoot != null)
        {
            Renderer renderer = FollowEntity.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                Bounds bounds = renderer.bounds;
                Vector3 toCamera = dirToCamera.normalized;
                Vector3 extents = bounds.extents;

                charOffset = Mathf.Abs(Vector3.Dot(extents, toCamera));
                charOffset = Mathf.Max(charOffset, 0.5f);
                
                float characterSize = bounds.extents.magnitude;
                minDistanceFromChar = Mathf.Max(characterSize * 1.5f, 4f);
            }
        }

        Vector3 adjustedStartPos = pivotPos + dirToCamera * charOffset;
        float maxCastDistance = Mathf.Max(desiredDistance - charOffset, Config.CAMERA_MIN_DISTANCE);
        Ray ray = new Ray(adjustedStartPos, dirToCamera);
        RaycastHit[] hits = Physics.SphereCastAll(
            ray,
            Config.CAMERA_COLLISION_RADIUS,
            maxCastDistance,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        if (hits == null || hits.Length == 0)
        {
            return Mathf.Max(desiredDistance, minDistanceFromChar);
        }
            
        float closest = desiredDistance;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider col = hits[i].collider;

            if (playerRoot != null && (col.transform == playerRoot || col.transform.IsChildOf(playerRoot)))
                continue;

            if (!col.CompareTag("Wall"))
                continue;

            float actualDistance = hits[i].distance + charOffset;
            float candidate = Mathf.Max(actualDistance - Config.CAMERA_COLLISION_BUFFER, minDistanceFromChar);
            
            if (candidate < closest)
                closest = candidate;
        }

        return Mathf.Clamp(closest, minDistanceFromChar, desiredDistance);
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float rightStickZoom = Input.GetAxis("VerticalRightJoystick");

        if (Input.GetButton("R3"))
        {
            zoom -= rightStickZoom * Config.ZOOM_SENSITIVITY * Time.deltaTime;
        }
        else
        {
            zoom -= scroll * Config.ZOOM_SENSITIVITY;
        }

        zoom = Mathf.Clamp(zoom, Config.CAMERA_MIN_ZOOM, maxZoom);
    }

    private void HandleRotation()
    {
        // Mouse input
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Controller input
        float rightStickX = Input.GetAxis("HorizontalRightJoystick");
        float rightStickY = Input.GetAxis("VerticalRightJoystick");

        // Combine both (so either works)
        float inputX = mouseX + rightStickX;
        float inputY = mouseY + rightStickY;

        yaw += inputX * Config.MOUSE_SENSITIVITY * Time.deltaTime;
        pitch -= inputY * Config.MOUSE_SENSITIVITY * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, Config.MIN_PITCH, Config.MAX_PITCH);
    }

    public override void PanTo(float zoomSize) => zoom = zoomSize;

    public override void SetMaxZoom(float max) => maxZoom = max;

    public void SetCameraFOV(float newFOV) => targetFOV = newFOV;
}