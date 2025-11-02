using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class Platform : TriggerableAbstract
{
    [Header("Movement Settings")]
    public float distance = 3f;
    public float speed = 2f;

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool movingUp;

    private Rigidbody rb;
    private BoxCollider boxCollider;

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
        Vector3 nextPosition = Vector3.MoveTowards(rb.position, target, moveStep);

        if (!movingUp)
        {
            Vector3 boxHalfExtents = boxCollider.bounds.extents;
            Vector3 origin = rb.position;

            if (Physics.BoxCast(origin, boxHalfExtents, Vector3.down, out RaycastHit hit, transform.rotation, moveStep))
            {
                if (!hit.collider.isTrigger && !hit.collider.transform.IsChildOf(transform))
                {
                    nextPosition.y = hit.point.y + boxHalfExtents.y + 0.01f;
                }
            }
        }

        rb.MovePosition(nextPosition);

        if (Vector3.Distance(rb.position, target) < 0.001f && IsOn)
        {
            IsOn = false;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!movingUp) return;

        Rigidbody rbOther = collision.rigidbody;
        if (rbOther != null)
        {
            Vector3 lv = rbOther.linearVelocity;
            lv.y = Mathf.Max(lv.y, speed);
            rbOther.linearVelocity = lv;
        }
    }

    public override void TurnOn() => IsOn = true;
    public override void TurnOff() => IsOn = false;

    private void OnDrawGizmosSelected()
    {
        if (boxCollider != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, boxCollider.bounds.size);
        }
    }
}
