using UnityEngine;
using UnityEngine.UI;

public class WorldTextFollower : MonoBehaviour
{
    public Transform target;
    private RectTransform rectTransform;
    

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 screenPos = CameraManager.Instance.Cam.WorldToScreenPoint(target.position + Config.INTERACTABLE_TEXT_OFFSET);

        if (screenPos.z < 0)
        {
            GetComponent<Text>().enabled = false;
            return;
        }

        GetComponent<Text>().enabled = true;
        rectTransform.position = screenPos;
    }
}
