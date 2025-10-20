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

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetChildrenActive(false);
    }

    void Update()
    {
        if (!isActive)
        {
            transform.position = PlayerMouse.Instance.getActivePlayer().transform.position;
        }
        else
        {
            float h = Input.GetAxis(_Input.Horizontal);
            float v = Input.GetAxis(_Input.Vertical);

            Transform camTransform = CameraManager.Instance.transform;
            Vector3 camForward = camTransform.forward;
            Vector3 camRight = camTransform.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 inputDir = (camForward * v + camRight * h).normalized;

            Vector3 intended = transform.position + inputDir * Config.PLAYER_MARKER_MOVE_SPEED * Time.deltaTime;

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

        RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, rayDistance, Config.PLAYER_MARKER_GROUND_LAYERS.value);
        if (hits == null || hits.Length == 0) return false;

        foreach (var hit in hits)
        {
            if (hit.collider != null && !hit.collider.isTrigger)
            {
                if (hit.collider.CompareTag("Environment"))
                {
                    if (hit.point.y > highestY) highestY = hit.point.y;
                }
            }
        }

        return highestY != float.MinValue;
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

        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 1f, NavMesh.AllAreas))
        {
            if (groundTarget == null)
            {
                groundTarget = new GameObject("GroundTarget");
            }
            groundTarget.transform.position = hit.position;
            SetTarget(groundTarget);
        }
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