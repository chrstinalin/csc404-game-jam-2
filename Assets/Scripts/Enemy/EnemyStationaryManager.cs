using Unity.VisualScripting;
using UnityEngine;
using System.Collections;

public class EnemyStationaryManager : MonoBehaviour, IOffense
{
    public EnemyVisionAbstractManager VisionManager;

    [SerializeField] private float rotationAngle = 45f;
    [SerializeField] private float rotationDuration = 2;
    [SerializeField] private float pauseDuration = 3;

    private Quaternion initialRotation;

    void Start()
    {
        VisionManager = GetComponent<EnemyVisionAbstractManager>();
        VisionManager.InitVision();
        initialRotation = transform.rotation;
        StartCoroutine(RotateBackAndForth());
    }

    void Update()
    {
        VisionManager.UpdateVision();
        //if (isAttack()) Debug.Log("Attacking!");
    }

    public bool isAttack()
    {
        return VisionManager.MouseIsSpotted || VisionManager.MechIsSpotted;
    }

    private IEnumerator RotateBackAndForth()
    {
        while (true)
        {
            yield return RotateToAngle(rotationAngle);
            yield return new WaitForSeconds(pauseDuration);
            yield return RotateToAngle(-rotationAngle);
            yield return new WaitForSeconds(pauseDuration);
        }
    }

    private IEnumerator RotateToAngle(float targetAngle)
    {
        float elapsed = 0f;
        float startAngle = transform.localEulerAngles.y;
        if (startAngle > 180f) startAngle -= 360f;
        float endAngle = targetAngle;

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / rotationDuration);
            float angle = Mathf.Lerp(startAngle, endAngle, t);
            transform.rotation = initialRotation * Quaternion.Euler(0, angle, 0);
            yield return null;
        }
        transform.rotation = initialRotation * Quaternion.Euler(0, endAngle, 0);
    }
}
