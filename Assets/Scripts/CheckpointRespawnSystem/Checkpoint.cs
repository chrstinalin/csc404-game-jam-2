using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Transform spawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        var manager = CheckpointManager.Instance;
        if (manager == null) return;

        if (other.gameObject == manager.mecha || other.gameObject == manager.mouse)
        {
            manager.SetActiveCheckpoint(this);
        }
    }
}
