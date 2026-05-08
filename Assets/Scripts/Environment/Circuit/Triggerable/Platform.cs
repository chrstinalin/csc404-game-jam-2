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
        Vector3 directionVector = target - rb.position;
        movingUp = directionVector.y > 0f;

        float moveStep = speed * Time.fixedDeltaTime;

        Vector3 desiredPosition = Vector3.MoveTowards(rb.position, target, moveStep);

        if (!movingUp)
        {
            if (IsBlockedDown(moveStep))
            {
                desiredPosition = rb.position;
            }
        }

        rb.MovePosition(desiredPosition);

        if (Vector3.Distance(rb.position, target) < 0.001f && IsOn)
        {
            TurnOff();
        }

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

    private bool IsBlockedDown(float distance)
    {
        Bounds bounds = boxCollider.bounds;

        Vector3 origin = bounds.center;
        origin.y = bounds.min.y + 0.01f;

        Vector3 halfExtents = bounds.extents;
        halfExtents.y = 0.02f; // thin bottom slice

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

    public override void TurnOn()
    {
        if (!IsOn)
        {
            IsOn = true;
        }
    }

    public override void TurnOff()
    {
        if (IsOn)
        {
            IsOn = false;
        }
    }

    private void OnDestroy()
    {
        if (isSFXPlaying)
        {
            AudioManager.Instance.StopSFX(movingSFXInstance);
        }
    }
}