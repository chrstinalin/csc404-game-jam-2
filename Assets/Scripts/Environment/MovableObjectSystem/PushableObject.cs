using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class PushableObject : MonoBehaviour 
{
    public SideTrigger[] sideTriggers;
    public float moveSpeed = 2f;
    [SerializeField] public EventReference boxPushSFX;
    public TopTrigger topTrigger;
    private float pushRadius = 2.3f;

    private Rigidbody rb;
    private Rigidbody mechRb;
    private MovementManager movementManager;
    private Animator mechAnimator;

    private bool isBeingPushed;
    private SideTrigger activeTrigger;
    private Vector3 pushAxis;
    private Vector3 mechFacingDir;
    private float mouseStartY;
    private float mechOffsetDistance;
    private float mechSideSign;

    private float hoverY;
    private const float hoverHeight = 0.2f;

    // FMOD push sound
    private EventInstance boxPushSFXInstance;
    private bool isSFXPlaying = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation 
                       | RigidbodyConstraints.FreezePositionX 
                       | RigidbodyConstraints.FreezePositionZ;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void Start()
    {
        if (PlayerMech.Instance != null)
        {
            mechAnimator = PlayerMech.Instance.GetComponentInChildren<Animator>();
        }

        movementManager = MovementManager.Instance;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Interact"))
            TryStartPush();

        if (Input.GetButton("Interact") && isBeingPushed)
            UpdatePush();

        if (Input.GetButtonUp("Interact") && isBeingPushed)
            StopPush();

        if (isBeingPushed)
        {
            float hoverForce = 20f;
            float hoverDamping = 5f;

            float heightError = hoverY - rb.position.y;
            float upwardForce = heightError * hoverForce - rb.linearVelocity.y * hoverDamping;

            rb.AddForce(Vector3.up * upwardForce, ForceMode.Acceleration);
        }
        else
        {
            bool onPlatform = IsOnPlatform();

            if (!onPlatform)
            {
                Vector3 vel = rb.linearVelocity;

                if (vel.y > 0f)
                    vel.y *= 0.5f;
                else
                    vel.y = Mathf.Max(vel.y, -2f);

                rb.linearVelocity = vel;
            }
        }

        CheckIfMouseIsOnTop();
    }

    // Move mech toward the box instead of moving the box
    private void SnapBoxToMech()
    {
        if (mechRb == null || rb == null) return;

        float desiredDistance = 1f;
        Vector3 targetPos = rb.position - (pushAxis * mechSideSign * desiredDistance);
        targetPos.y = mechRb.position.y; // Keep mech's Y the same for vertical alignment

        mechRb.position = targetPos;
    }

    private void TryStartPush()
    {
        if (movementManager.IsMouseActive || isBeingPushed)
            return;

        activeTrigger = null;
        foreach (var t in sideTriggers)
        {
            if (t.CanPush())
            {
                activeTrigger = t;
                break;
            }
        }

        if (activeTrigger == null) return;

        mechRb = PlayerMech.Instance.GetComponent<Rigidbody>();
        if (mechRb == null) return;

        if (Vector3.Distance(mechRb.position, rb.position) > pushRadius) return;

        if (topTrigger != null && topTrigger.mouseOnTop)
            mouseStartY = PlayerMouse.Instance.transform.position.y;

        pushAxis = GetAxisFromSide(activeTrigger.side);

        Vector3 mechToBox = rb.position - mechRb.position;
        float signedDistance = Vector3.Dot(mechToBox, pushAxis);
        mechSideSign = Mathf.Sign(signedDistance);
        mechOffsetDistance = Mathf.Abs(signedDistance);

        mechFacingDir = pushAxis * mechSideSign;

        SnapBoxToMech(); // now mech moves to the box

        hoverY = rb.position.y + hoverHeight;

        rb.useGravity = true;

        isBeingPushed = true;
        movementManager.isLockedMovement = true;

        rb.constraints = RigidbodyConstraints.FreezeRotation;
        mechRb.constraints = RigidbodyConstraints.FreezeRotation;

        isSFXPlaying = false; // will start in UpdatePush based on horizontal velocity
    }

    private void UpdatePush()
    {
        if (Vector3.Distance(mechRb.position, rb.position) > pushRadius)
        {
            StopPush();
            return;
        }

        Camera cam = movementManager.CameraManager.Cam;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 camForward = cam.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = cam.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 moveDir = camForward * v + camRight * h;

        float axisInput = Vector3.Dot(moveDir, pushAxis);

        float pushSpeed = Config.MECH_MOVE_SPEED * 0.5f;

        float moveMagnitude = Mathf.Abs(axisInput) < 0.01f ? 0f : pushSpeed * Mathf.Sign(axisInput);

        Vector3 moveDirNormalized = moveDir.sqrMagnitude > 0.001f ? moveDir.normalized : Vector3.zero;

        float forwardDot = Vector3.Dot(moveDirNormalized, mechFacingDir);

        mechAnimator.SetBool("isRunning", Mathf.Abs(moveMagnitude) > 0.01f);
        mechAnimator.SetBool("isWalkingBackwards", forwardDot < -0.1f);

        Vector3 velocity = pushAxis * moveMagnitude;

        mechRb.linearVelocity = new Vector3(velocity.x, mechRb.linearVelocity.y, velocity.z);
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);

        // --- FMOD: play sound only when being pushed and horizontal velocity > 0 ---
        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (isBeingPushed && horizontalVel.magnitude > 0.01f)
        {
            if (!isSFXPlaying)
            {
                boxPushSFXInstance = AudioManager.Instance.PlaySFX(boxPushSFX, transform.position, 1f);
                isSFXPlaying = true;
            }
        }
        else
        {
            if (isSFXPlaying)
            {
                AudioManager.Instance.StopSFX(boxPushSFXInstance);
                isSFXPlaying = false;
            }
        }

        float rotateSpeed = 720f;
        Quaternion targetRot = Quaternion.LookRotation(pushAxis * mechSideSign, Vector3.up);
        mechRb.rotation = Quaternion.RotateTowards(mechRb.rotation, targetRot, rotateSpeed * Time.deltaTime);
    }

    private void StopPush()
    {
        isBeingPushed = false;
        activeTrigger = null;

        mechAnimator.SetBool("isRunning", false);

        movementManager.isLockedMovement = false;

        rb.linearVelocity = Vector3.zero;
        mechRb.linearVelocity = Vector3.zero;

        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        mechRb.constraints = RigidbodyConstraints.FreezeRotation;

        rb.useGravity = true;

        if (isSFXPlaying)
        {
            AudioManager.Instance.StopSFX(boxPushSFXInstance);
            isSFXPlaying = false;
        }
    }

    private Vector3 GetAxisFromSide(CardinalDirection side)
    {
        switch (side)
        {
            case CardinalDirection.East:
            case CardinalDirection.West: 
                return Vector3.right;

            case CardinalDirection.North:
            case CardinalDirection.South: 
                return Vector3.forward;
                
            default: return Vector3.zero;
        }
    }

    private void CheckIfMouseIsOnTop()
    {
        if (PlayerMouse.Instance == null || topTrigger == null) return;

        if (topTrigger.mouseOnTop && isBeingPushed)
        {
            if (PlayerMouse.Instance.transform.parent != transform)
                PlayerMouse.Instance.transform.SetParent(transform);

            Vector3 mousePos = PlayerMouse.Instance.transform.position;
            mousePos.y = mouseStartY;
            PlayerMouse.Instance.transform.position = mousePos;
        }
        else
        {
            if (PlayerMouse.Instance.transform.parent == transform)
                PlayerMouse.Instance.transform.SetParent(null);
        }
    }

    private bool IsOnPlatform()
    {
        float rayDistance = 0.3f;

        if (Physics.Raycast(rb.position, Vector3.down, out RaycastHit hit, rayDistance))
        {
            return hit.collider.GetComponentInChildren<Platform>() != null;
        }

        return false;
    }
}