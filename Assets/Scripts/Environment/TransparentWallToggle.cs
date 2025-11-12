using UnityEngine;

public class TransparentWallToggle : MonoBehaviour
{
    public GameObject objectIfWall;
    public GameObject objectIfClear;

    void Update()
    {
        if (objectIfWall == null || objectIfClear == null)
            return;

        CheckWalls();
    }

    void CheckWalls()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        if (PlayerMech.Instance == null) return;
        Transform target = PlayerMech.Instance.transform;

        Vector3 direction = target.position - mainCamera.transform.position;
        float distance = direction.magnitude;
        direction.Normalize();

        RaycastHit[] hits = Physics.RaycastAll(mainCamera.transform.position, direction, distance);

        bool wallInTheWay = false;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Wall"))
            {
                wallInTheWay = true;
                break;
            }
        }

        objectIfWall.SetActive(wallInTheWay);
        objectIfClear.SetActive(!wallInTheWay);
    }
}
