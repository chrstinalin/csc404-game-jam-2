using UnityEngine;
using System.Collections;

public class EnemyStationaryManager : MonoBehaviour, IOffense
{
    public EnemyVisionAbstractManager VisionManager;

    [SerializeField] private float rotationAngle = 45f;
    [SerializeField] private float rotationDuration = 2f;
    [SerializeField] private float pauseDuration = 2f;

    private Quaternion leftRotation;
    private Quaternion rightRotation;
    private bool rotatingRight = true;

    void Start()
    {
        VisionManager = GetComponent<EnemyVisionAbstractManager>();
        VisionManager.InitVision();

        Quaternion baseRotation = transform.rotation;
        leftRotation = baseRotation * Quaternion.Euler(0f, -rotationAngle, 0f);
        rightRotation = baseRotation * Quaternion.Euler(0f, rotationAngle, 0f);

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
            Quaternion targetRotation = rotatingRight ? rightRotation : leftRotation;
            Quaternion startRotation = transform.rotation;

            float elapsed = 0f;

            while (elapsed < rotationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / rotationDuration);
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                yield return null;
            }

            transform.rotation = targetRotation;
            rotatingRight = !rotatingRight;

            yield return new WaitForSeconds(pauseDuration);
        }
    }
}
