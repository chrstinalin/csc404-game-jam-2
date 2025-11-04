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

    void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        Cam = GetComponent<Camera>();
        CameraPivot = transform.parent;
        heightOffset = CameraPivot.position.y;
        targetFOV = Config.CAMERA_DEFAULT_FOV;

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Cam.fieldOfView = Mathf.SmoothDamp(Cam.fieldOfView, targetFOV, ref fovVelocity, Config.CAMERA_SMOOTH_TIME);
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

        CameraPivot.position = new Vector3(FollowEntity.transform.position.x,
                                           FollowEntity.transform.position.y + heightOffset,
                                           FollowEntity.transform.position.z);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        zoom -= scroll * Config.ZOOM_SENSITIVITY;

        bool r3Pressed = Input.GetButton("R3");
        if (r3Pressed)
        {
            float rightStickY = Input.GetAxis("VerticalRightJoystick");
            zoom -= rightStickY * Config.ZOOM_SENSITIVITY * Time.deltaTime;
        }

        zoom = Mathf.Clamp(zoom, Config.CAMERA_MIN_ZOOM, maxZoom);

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        float rightStickX = Input.GetAxis("HorizontalRightJoystick");

        float inputX = mouseX + (r3Pressed ? 0 : rightStickX);
        float inputY = mouseY;

        yaw += inputX * Config.MOUSE_SENSITIVITY * Time.deltaTime;
        pitch -= inputY * Config.MOUSE_SENSITIVITY * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, Config.MIN_PITCH, Config.MAX_PITCH);

        Quaternion targetRotation = Quaternion.Euler(pitch, yaw, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Config.SMOOTH_TIME);

        transform.position = CameraPivot.position - transform.forward * zoom;
    }

    public override void PanTo(float zoomSize) => zoom = zoomSize;

    public override void SetMaxZoom(float max) => maxZoom = max;

    public void SetCameraFOV(float newFOV) => targetFOV = newFOV;
}
