using UnityEngine;
using FMODUnity;
using FMOD.Studio;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class Platform : TriggerableAbstract
{
    [Header("Movement Settings")]
    public float distance = 3f;
    public float speed = 2f;
    public bool carryObjects = true;

    [Header("FMOD Settings")]
    [SerializeField] private EventReference movingSFX;

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool movingUp;

    private Rigidbody rb;
    private BoxCollider boxCollider;

    private EventInstance movingSFXInstance;
    private bool isSFXPlaying = false;

    private float? clampLimitOverride = null;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        boxCollider = GetComponent<BoxCollider>();
    }

    private void Start()
    {
        startPos = rb.position;
        targetPos = startPos + Vector3.up * distance;
    }

    private void FixedUpdate()
    {
        Vector3 target = IsOn ? targetPos : startPos;

        Vector3 direction = target - rb.position;
        movingUp = direction.y > 0f;

        float moveStep = speed * Time.fixedDeltaTime;

        Vector3 desiredPosition = Vector3.MoveTowards(rb.position, target, moveStep);

        float physicsLimitY = GetPhysicsLimitY(moveStep);

        float clampLimitY = clampLimitOverride.HasValue
            ? clampLimitOverride.Value
            : float.NegativeInfinity;

        float finalY = desiredPosition.y;

        finalY = Mathf.Max(finalY, physicsLimitY, clampLimitY);

        desiredPosition.y = finalY;

        rb.MovePosition(desiredPosition);

        float velocityMagnitude = (rb.position - desiredPosition).magnitude / Time.fixedDeltaTime;

        if (velocityMagnitude > 0.01f)
        {
            if (!isSFXPlaying)
            {
                movingSFXInstance = AudioManager.Instance.PlaySFX(movingSFX, transform.position);
                isSFXPlaying = true;
            }
        }
        else
        {
            if (isSFXPlaying)
            {
                AudioManager.Instance.StopSFX(movingSFXInstance);
                isSFXPlaying = false;
            }
        }
    }

    private float GetPhysicsLimitY(float distance)
    {
        if (!movingUp && IsBlockedDown(distance))
        {
            return rb.position.y;
        }

        return float.NegativeInfinity;
    }

    private bool IsBlockedDown(float distance)
    {
        Bounds bounds = boxCollider.bounds;

        Vector3 origin = bounds.center;
        origin.y = bounds.min.y + 0.01f;

        Vector3 halfExtents = bounds.extents;
        halfExtents.y = 0.02f;

        float castDistance = distance + 0.05f;

        RaycastHit hit;

        bool blocked = Physics.BoxCast(
            origin,
            halfExtents,
            Vector3.down,
            out hit,
            Quaternion.identity,
            castDistance
        );

        if (!blocked) return false;
        if (hit.collider == null) return false;
        if (hit.collider == boxCollider) return false;
        if (hit.collider.isTrigger) return false;

        return true;
    }

    public void SetClamp(float? height)
    {
        clampLimitOverride = height;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!carryObjects) return;
        if (!movingUp) return;

        Rigidbody rbOther = collision.rigidbody;
        if (rbOther == null) return;

        foreach (ContactPoint contact in collision.contacts)
        {
            if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f)
            {
                Vector3 lv = rbOther.linearVelocity;
                lv.y = Mathf.Max(lv.y, speed);
                rbOther.linearVelocity = lv;
                return;
            }
        }
    }

    public override void TurnOn() => IsOn = true;
    public override void TurnOff() => IsOn = false;

    private void OnDestroy()
    {
        if (isSFXPlaying)
        {
            AudioManager.Instance.StopSFX(movingSFXInstance);
        }
    }
}