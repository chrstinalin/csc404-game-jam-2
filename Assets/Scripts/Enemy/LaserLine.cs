using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class LaserLine : MonoBehaviour
{
    public float maxLength = 20f;
    public List<GameObject> obstacles;
    public float laserWidth = 0.5f;

    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = laserWidth;
        lineRenderer.endWidth = laserWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = false;
    }

    void Update()
    {
        UpdateLaser();
    }

    void UpdateLaser()
    {
        float laserLength = maxLength;
        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, maxLength);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            if (hit.collider.isTrigger)
                continue;

            GameObject hitObject = hit.collider.gameObject;

            bool isPlayer =
                hit.collider.GetComponentInParent<PlayerMech>() != null ||
                hit.collider.GetComponentInParent<PlayerMouse>() != null;

            if (isPlayer)
            {
                DamageReceiver damageReceiver = hit.collider.GetComponent<DamageReceiver>();
                if (damageReceiver == null)
                    damageReceiver = hit.collider.GetComponentInParent<DamageReceiver>();

                if (damageReceiver != null)
                    damageReceiver.ReceiveDamage(int.MaxValue, gameObject);

                laserLength = hit.distance;
                break;
            }
            else if (obstacles.Contains(hitObject))
            {
                laserLength = hit.distance;
                break;
            }
        }

        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, Vector3.forward * laserLength);
    }
}
