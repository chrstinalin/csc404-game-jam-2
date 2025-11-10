using System;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMarker : MonoBehaviour
{
    [NonSerialized] public static PlayerMarker Instance;
    [NonSerialized] public GameObject Target;

    public event Action<GameObject> OnTargetChanged;

    private Joystick _Input = Constant.JOY_LEFT;
    private bool isActive = false;
    private GameObject groundTarget;

    private int overlappingSelectables = 0;
    private bool isOverSelectable => overlappingSelectables > 0;

    [SerializeField] private LayerMask wallLayers = 0;
    [SerializeField] private float wallSkin = 0f;
    private SphereCollider collider;

    void Awake()
    {
        Instance = this;
        collider = GetComponent<SphereCollider>();
    }

    private void Start()
    {
        SetChildrenActive(false);
    }

    void Update()
    {
        bool playerControllingMech = MovementManager.Instance != null && !MovementManager.Instance.IsMouseActive;
        if (!playerControllingMech)
        {
            if (isActive) setActive(false);
            return;
        }

        if (!isActive)
        {
            transform.position = PlayerMouse.Instance.getActivePlayer().transform.position;
        }
        else
        {
            float h = Input.GetAxis(_Input.Horizontal);
            float v = Input.GetAxis(_Input.Vertical);

            Camera cam = Camera.main;
            Vector3 camForward = cam.transform.forward;
            Vector3 camRight = cam.transform.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 inputDir = (camForward * v + camRight * h).normalized;

            Vector3 startPos = transform.position;
            Vector3 horizontalTarget = startPos + inputDir * Config.PLAYER_MARKER_MOVE_SPEED * Time.deltaTime;
            Vector3 intended = ResolveWallCollisions(startPos, horizontalTarget);

            if (TryGetHighestGroundY(intended, out float groundY))
            {
                intended.y = groundY + Config.PLAYER_MARKER_GROUND_SNAP_OFFSET;
                transform.position = intended;
            }
            else
            {
                Vector3 fallback = transform.position;
                fallback.x = intended.x;
                fallback.z = intended.z;
                transform.position = fallback;
            }
        }

        if (isActive && Input.GetButtonDown("Interact"))
        {
            TryLockOnGround();
        }
    }


    public void setActive(bool val)
    {
        CameraManager.Instance.SetFollowEntity(gameObject, null);
        SetChildrenActive(val);
        isActive = val;
        
        if (!val)
        {
            overlappingSelectables = 0;
        }
    }

    private void SetChildrenActive(bool val)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child != null)
            {
                child.gameObject.SetActive(val);
            }
        }
    }

    private bool TryGetHighestGroundY(Vector3 positionXZ, out float highestY)
    {
        highestY = float.MinValue;
        float originY = positionXZ.y + Config.PLAYER_MARKER_GROUND_RAY_HEIGHT;
        Vector3 origin = new Vector3(positionXZ.x, originY, positionXZ.z);
        float rayDistance = Config.PLAYER_MARKER_GROUND_RAY_HEIGHT * 2f;

        RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, rayDistance, Config.PLAYER_MARKER_GROUND_LAYERS.value, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0) return false;

        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;
            if (hit.collider.isTrigger) continue;
            if (hit.collider.CompareTag("Wall")) continue;
            if (hit.point.y > highestY)
            {
                highestY = hit.point.y;
            }
        }

        return highestY != float.MinValue;
    }

    private Vector3 ResolveWallCollisions(Vector3 start, Vector3 intended)
    {
        Vector3 moveXZ = intended - start;
        moveXZ.y = 0f;
        float distance = moveXZ.magnitude;
        if (distance <= Mathf.Epsilon)
        {
            return intended;
        }

        Vector3 dir = moveXZ / distance;

        Vector3 origin = start + (collider != null ? (transform.rotation * collider.center) : (Vector3.up * 0.5f));
        float radius = (collider != null) ? Mathf.Max(0f, collider.radius - wallSkin) : 0f;
        RaycastHit[] hits = radius > 0f
            ? Physics.SphereCastAll(origin, radius, dir, distance, wallLayers, QueryTriggerInteraction.Ignore)
            : Physics.RaycastAll(origin, dir, distance, wallLayers, QueryTriggerInteraction.Ignore);

        if (hits != null && hits.Length > 0)
        {
            float closest = float.PositiveInfinity;
            foreach (var h in hits)
            {
                if (h.collider == null || !h.collider.CompareTag("Wall")) continue;
                if (h.distance < closest) closest = h.distance;
            }
            if (closest < float.PositiveInfinity)
            {
                float allowed = Mathf.Max(0f, closest - wallSkin);
                return start + dir * allowed;
            }
        }

        return intended;
    }


    void OnTriggerEnter(Collider other)
    {
        var selectable = other.GetComponent<LockOnSelectable>();
        if (selectable != null && selectable.enabled)
        {
            selectable.OnHover(true);
            overlappingSelectables++;
        }
    }

    void OnTriggerExit(Collider other)
    {
        var selectable = other.GetComponent<LockOnSelectable>();
        if (selectable != null)
        {
            selectable.OnHover(false);
            overlappingSelectables = Mathf.Max(0, overlappingSelectables - 1);
        }
    }

    public void SetTarget(GameObject target)
    {
        Target = target;
        OnTargetChanged?.Invoke(Target);
    }

    private void TryLockOnGround()
    {
        bool actuallyOverSelectable = CheckForActiveSelectables();
        if (actuallyOverSelectable) return;
    }

    private bool CheckForActiveSelectables()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1f);
        int validSelectables = 0;
        foreach (Collider col in colliders)
        {
            LockOnSelectable selectable = col.GetComponent<LockOnSelectable>();
            if (selectable != null && selectable.enabled && selectable.gameObject.activeInHierarchy)
            {
                validSelectables++;
            }
        }
        overlappingSelectables = validSelectables;
        return validSelectables > 0;
    }

    public void ClearTarget()
    {
        Target = null;
        OnTargetChanged?.Invoke(null);
    }
}