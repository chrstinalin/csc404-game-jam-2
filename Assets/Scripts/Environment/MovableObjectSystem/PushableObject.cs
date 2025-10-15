using UnityEngine;

public class PushableObject : MovableObject
{
    public float floorHeight = 0f;
    public SideTrigger[] sideTriggers;
    public GameObject player;

    private void Awake()
    {
        foreach (var t in sideTriggers)
            t.player = player;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Interact"))
            TryPush();
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

        transform.position = targetPos;
        Vector3 pos = transform.position;
        pos.y = floorHeight;
        transform.position = pos;
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
