using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ShopPatrol : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Vector3 moveDirection = Vector3.forward;

    private Rigidbody rb;
    private Vector3 currentDirection;
    private Vector3 lastPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        currentDirection = moveDirection.normalized;

        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        rb.constraints =
            RigidbodyConstraints.FreezePositionX |
            RigidbodyConstraints.FreezePositionY |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY |
            RigidbodyConstraints.FreezeRotationZ;

        lastPosition = rb.position;
    }
    private void FixedUpdate()
    {
        // Prevent getting stuck by security door
        float distanceMoved = Vector3.Distance(rb.position, lastPosition);
        if (distanceMoved < 0.001f)
        {
            SwitchDirection();
        }

        lastPosition = rb.position;

        rb.MovePosition(
            rb.position + currentDirection * moveSpeed * Time.fixedDeltaTime
        );
    }

    private void OnCollisionEnter(Collision collision)
    {
        SwitchDirection();
    }

    private void SwitchDirection()
    {
        currentDirection = -currentDirection;

        rb.angularVelocity = Vector3.zero;
        transform.rotation *= Quaternion.Euler(0f, 180f, 0f);
    }
}