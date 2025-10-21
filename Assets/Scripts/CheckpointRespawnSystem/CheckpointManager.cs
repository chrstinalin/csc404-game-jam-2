using UnityEngine;
using System.Collections.Generic;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    public GameObject mecha;
    public GameObject mouse;

    public List<Checkpoint> checkpoints = new List<Checkpoint>();

    private Checkpoint currentCheckpoint;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (checkpoints != null && checkpoints.Count > 0 && checkpoints[0] != null)
        {
            currentCheckpoint = checkpoints[0];
            Debug.Log($"Starting at checkpoint: {currentCheckpoint.name}");
        }
        else
        {
            Debug.LogWarning("No checkpoints assigned in the list!");
        }
    }

    public void SetActiveCheckpoint(Checkpoint checkpoint)
    {
        int newIndex = checkpoints.IndexOf(checkpoint);
        int currentIndex = currentCheckpoint ? checkpoints.IndexOf(currentCheckpoint) : -1;

        if (newIndex > currentIndex)
        {
            currentCheckpoint = checkpoint;
            Debug.Log($"Activated checkpoint: {currentCheckpoint.name}");
        }
    }

    public void RespawnCharacters()
    {
        if (currentCheckpoint == null || currentCheckpoint.spawnPoint == null)
        {
            Debug.LogWarning("No active checkpoint to respawn at!");
            return;
        }

        Vector3 spawnPos = currentCheckpoint.spawnPoint.position;
        Quaternion spawnRot = currentCheckpoint.spawnPoint.rotation;

        Vector3 offset = Vector3.right * 1.5f;

        mecha.transform.SetPositionAndRotation(spawnPos + offset, spawnRot);
        mouse.transform.SetPositionAndRotation(spawnPos - offset, spawnRot);

        if (mecha.TryGetComponent<Rigidbody>(out var rb1))
            rb1.linearVelocity = Vector3.zero;

        if (mouse.TryGetComponent<Rigidbody>(out var rb2))
            rb2.linearVelocity = Vector3.zero;

        Debug.Log($"Respawned both characters at checkpoint: {currentCheckpoint.name}, position: {spawnPos}");
    }

    public bool CurrentCheckpointExists => currentCheckpoint != null;

    public Transform GetCurrentSpawnPoint()
    {
        return currentCheckpoint != null ? currentCheckpoint.spawnPoint : null;
    }
}
