using UnityEngine;
using FMODUnity;

public class PushableObject : MovableObject
{
    public SideTrigger[] sideTriggers;
    public float moveSpeed = 2f;
    [SerializeField] public EventReference boxPushSFX;
    public TopTrigger topTrigger;
    public float pushRadius = 0.1f;

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
    private const float hoverHeight = 0.05f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
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
            Vector3 pos = rb.position;
            pos.y = hoverY;
            rb.position = pos;

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        }

        CheckIfMouseIsOnTop();
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

        // Set hover height. The hover is so the box can go over tiny platforms
        hoverY = rb.position.y + hoverHeight;
        rb.useGravity = false;

        isBeingPushed = true;
        movementManager.isLockedMovement = true;

        rb.constraints = RigidbodyConstraints.FreezeRotation;
        mechRb.constraints = RigidbodyConstraints.FreezeRotation;

        AudioManager.Instance.PlaySFX(boxPushSFX, transform.position, 10f);
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

        // Constraint DK to only walk in a cardinal direction
        float axisInput = Vector3.Dot(moveDir, pushAxis);

        // Pushing speed is 50% DK walking speed
        float pushSpeed = Config.MECH_MOVE_SPEED * 0.5f;

        float moveMagnitude = Mathf.Abs(axisInput) < 0.01f ? 0f : pushSpeed * Mathf.Sign(axisInput);

        // Play walking animation if DK is moving
        mechAnimator.SetBool("isRunning", Mathf.Abs(moveMagnitude) > 0.01f);

        Vector3 velocity = pushAxis * moveMagnitude;

        mechRb.linearVelocity = new Vector3(velocity.x, mechRb.linearVelocity.y, velocity.z);
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
        
        // Rotate to look at the box
        float rotateSpeed = 720f;
        Quaternion targetRot = Quaternion.LookRotation(pushAxis * mechSideSign, Vector3.up);
        mechRb.rotation = Quaternion.RotateTowards(mechRb.rotation, targetRot, rotateSpeed * Time.deltaTime);
    }

    private void StopPush()
    {
        isBeingPushed = false;
        activeTrigger = null;

        // Stop walking animation
        mechAnimator.SetBool("isRunning", false);

        movementManager.isLockedMovement = false;

        rb.linearVelocity = Vector3.zero;
        mechRb.linearVelocity = Vector3.zero;

        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        mechRb.constraints = RigidbodyConstraints.FreezeRotation;

        rb.useGravity = true;
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
}
