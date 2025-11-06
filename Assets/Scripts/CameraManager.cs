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
    private new float zoom = 5f;
    private Vector2 rotationVelocity;
    private float maxZoom = Config.CAMERA_MAX_ZOOM;

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

        // Apply rotation and position
        Quaternion targetRotation = Quaternion.Euler(pitch, yaw, 0f);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Config.SMOOTH_TIME
        );

        transform.position = CameraPivot.position - transform.forward * zoom;
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
