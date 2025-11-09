using UnityEngine;
using System.Collections;

public class PushableObject : MovableObject
{
    public SideTrigger[] sideTriggers;
    public float moveSpeed = 2f;

    private bool isBeingPushed = false;
    private Rigidbody rb;
    private Vector3Int currentCell;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Start()
    {
        currentCell = grid.WorldToCell(transform.position);
        SnapToGrid();

        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Interact"))
            TryPush();

        if (!isBeingPushed)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        }
    }

    private void TryPush()
    {
        if (PlayerMech.Instance == null || isBeingPushed)
            return;

        SideTrigger trigger = null;
        foreach (var t in sideTriggers)
        {
            if (t.CanPush())
            {
                trigger = t;
                break;
            }
        }

        if (trigger == null)
            return;

        Vector3Int pushDir = GetPushDirection(trigger.side);
        Vector3Int targetCell = currentCell + pushDir;

        Vector3 targetPos = grid.GetCellCenterWorld(targetCell);

        Collider[] hits = Physics.OverlapBox(targetPos, Vector3.one * 0.45f);
        foreach (var c in hits)
        {
            if (c.transform.IsChildOf(transform) || c.gameObject == gameObject)
                continue;
            if (c.gameObject == PlayerMech.Instance.gameObject)
                continue;
            if (c.isTrigger)
                continue;

            return;
        }

        StartCoroutine(MoveBox(targetCell));
    }

    private IEnumerator MoveBox(Vector3Int targetCell)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.BoxMoveSFX);

        isBeingPushed = true;

        rb.constraints = RigidbodyConstraints.FreezeRotation;

        Vector3 startPos = rb.position;
        Vector3 endPos = grid.GetCellCenterWorld(targetCell);

        float distance = Vector3.Distance(startPos, endPos);
        float elapsed = 0f;

        Collider playerCol = PlayerMech.Instance.GetComponent<Collider>();
        Collider boxCol = GetComponent<Collider>();
        Physics.IgnoreCollision(boxCol, playerCol, true);

        while (elapsed < distance / moveSpeed)
        {
            elapsed += Time.deltaTime;
            Vector3 newPos = Vector3.Lerp(startPos, endPos, elapsed * moveSpeed / distance);
            rb.MovePosition(newPos);
            yield return null;
        }

        rb.MovePosition(endPos);
        currentCell = targetCell;

        Physics.IgnoreCollision(boxCol, playerCol, false);

        isBeingPushed = false;

        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
    }

    private Vector3Int GetPushDirection(CardinalDirection side)
    {
        switch (side)
        {
            case CardinalDirection.North: return GridDirection.North;
            case CardinalDirection.South: return GridDirection.South;
            case CardinalDirection.East:  return GridDirection.East;
            case CardinalDirection.West:  return GridDirection.West;
            default: return Vector3Int.zero;
        }
    }
}
