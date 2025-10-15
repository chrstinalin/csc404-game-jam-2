using UnityEngine;

public class Platform : TriggerableAbstract
{
    [Header("Movement Settings")]
    public PlatformMoveDirection direction = PlatformMoveDirection.Vertical;
    public float distance = 3f;
    public float speed = 2f;

    private Vector3 startPos;
    private Vector3 targetPos;

    private void Start()
    {
        startPos = transform.position;
        SetTargetPosition();
    }

    private void Update()
    {
        if (IsOn)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPos) < 0.01f)
                IsOn = false;
        }
        else
        {
            if (Vector3.Distance(transform.position, startPos) > 0.01f)
                transform.position = Vector3.MoveTowards(transform.position, startPos, speed * Time.deltaTime);
        }
    }

    private void SetTargetPosition()
    {
        if (direction == PlatformMoveDirection.Vertical)
            targetPos = startPos + Vector3.up * distance;
        else
            targetPos = startPos + Vector3.right * distance;
    }

    public override void TurnOn()
    {
        IsOn = true;
    }

    public override void TurnOff()
    {
        IsOn = false;
    }
}
