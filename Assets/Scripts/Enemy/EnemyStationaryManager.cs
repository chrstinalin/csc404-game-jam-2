using UnityEngine;
using System.Collections;

public class EnemyStationaryManager : MonoBehaviour, IOffense
{
    public EnemyVisionAbstractManager VisionManager;

    [SerializeField] private float rotationAngle = 45f;
    [SerializeField] private float rotationDuration = 2f;
    [SerializeField] private float pauseDuration = 3f;

    private Quaternion leftRotation;
    private Quaternion rightRotation;

    void Start()
    {
        VisionManager = GetComponent<EnemyVisionAbstractManager>();
        VisionManager.InitVision();

        // Cache the two target rotations relative to starting rotation
        leftRotation = transform.rotation * Quaternion.Euler(0f, -rotationAngle, 0f);
        rightRotation = transform.rotation * Quaternion.Euler(0f, rotationAngle, 0f);

        StartCoroutine(RotateBackAndForth());
    }

    void Update()
    {
        VisionManager.UpdateVision();
    }

    public bool isAttack()
    {
        return VisionManager.MouseIsSpotted || VisionManager.MechIsSpotted;
    }

    private IEnumerator RotateBackAndForth()
    {
        while (true)
        {
            yield return RotateTowards(rightRotation);
            yield return new WaitForSeconds(pauseDuration);
            yield return RotateTowards(leftRotation);
            yield return new WaitForSeconds(pauseDuration);
        }
    }

    private IEnumerator RotateTowards(Quaternion targetRot)
    {
        Quaternion startRot = transform.rotation;
        float elapsed = 0f;

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / rotationDuration);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        transform.rotation = targetRot;
    }
}
