using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class PlatformClampTrigger : MonoBehaviour
{
    [SerializeField] private Platform platform;
    [SerializeField] private float clampHeight = 0f;

    private void Awake()
    {
        if (platform == null)
            platform = GetComponentInParent<Platform>();
    }

    private bool IsPlayer(Collider other)
    {
        return PlayerMech.Instance != null &&
               other.GetComponentInParent<PlayerMech>() == PlayerMech.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;

        TryEnableClamp();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsPlayer(other)) return;

        TryEnableClamp();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;

        platform.SetClamp(null);
    }

    private void TryEnableClamp()
    {
        if (platform.transform.position.y > clampHeight)
        {
            platform.SetClamp(clampHeight);
        }
    }
}