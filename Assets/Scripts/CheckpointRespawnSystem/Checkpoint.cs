using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Transform spawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        var manager = CheckpointManager.Instance;
        if (manager == null) return;

        if (other.gameObject == PlayerMech.Instance.gameObject || other.gameObject == PlayerMouse.Instance.gameObject)
        {
            manager.SetActiveCheckpoint(this);
        }
    }
}
