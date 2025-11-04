using UnityEngine;

public class PushableObject : MovableObject
{
    public float floorHeight = 0f;
    public SideTrigger[] sideTriggers;
    public GameObject player;

    private bool isBeingPushed = false;
    private Vector3 lockedXZPosition;

    private void Awake()
    {
        foreach (var t in sideTriggers)
            t.player = player;
    }

    private void Start()
    {
        // Record the initial locked XZ position
        lockedXZPosition = new Vector3(transform.position.x, 0f, transform.position.z);
    }

    private void Update()
    {
        if (Input.GetButtonDown("Interact"))
            TryPush();

        // If not being pushed, lock X and Z positions
        if (!isBeingPushed)
        {
            Vector3 pos = transform.position;
            pos.x = lockedXZPosition.x;
            pos.z = lockedXZPosition.z;
            transform.position = pos;
        }
    }

    private void TryPush()
    {
        if (player == null)
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

        SnapToGrid();
        Vector3Int currentCell = grid.WorldToCell(transform.position);
        Vector3Int targetCell = currentCell + pushDir;
        Vector3 targetPos = grid.GetCellCenterWorld(targetCell);

        Collider[] hits = Physics.OverlapBox(targetPos, Vector3.one * 0.45f);
        foreach (var c in hits)
        {
            if (c.transform.IsChildOf(transform) || c.gameObject == gameObject)
                continue;

            if (c.gameObject == player)
                continue;

            if (c.isTrigger)
                continue;

            return;
        }

        // Temporarily allow X/Z movement
        isBeingPushed = true;

        // Move the object
        transform.position = targetPos;

        // Reset Y to floor height
        Vector3 pos = transform.position;
        pos.y = floorHeight;
        transform.position = pos;

        // Update locked position
        lockedXZPosition = new Vector3(transform.position.x, 0f, transform.position.z);

        // Lock again
        isBeingPushed = false;
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
