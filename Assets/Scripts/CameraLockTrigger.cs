using UnityEngine;

public class CameraLockTrigger : MonoBehaviour
{
    [SerializeField] private Vector3 lockedCameraPosition;
    [SerializeField] private Vector3 lockedCameraRotation; // Euler angles

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MousePlayerEntity"))
        {
            Quaternion rotation = Quaternion.Euler(lockedCameraRotation);
            CameraManager.Instance.SetCameraLock(lockedCameraPosition, true, rotation);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MousePlayerEntity"))
        {
            CameraManager.Instance.SetCameraLock(Vector3.zero, false); // Unlock camera
        }
    }
}