using UnityEngine;
using UnityEngine.UI;

public class LockOnUI : MonoBehaviour
{
    [Header("UI References")]
    public Image targetImage;
    public Text targetNameText;
    public Text targetDescriptionText;

    [Header("Image Settings")]
    [Range(0f, 1f)]
    public float imageOpacity = 0.1f;

    void Start()
    {
        HideUI();
    }

    public void UpdateUI(LockOnObject target)
    {
        if (target == null)
        {
            HideUI();
            return;
        }

        ShowUI();

        targetImage.sprite = target.sprite;
        targetImage.color = Color.green;

        targetNameText.text = target.displayName.ToUpper();
        targetDescriptionText.text = target.description;

        Color nameColor = targetNameText.color;
        targetNameText.color = nameColor;

        Color descColor = targetDescriptionText.color;
        targetDescriptionText.color = descColor;
    }
    private void HideUI()
    {
        targetImage.enabled = false;
        targetNameText.enabled = false;
        targetDescriptionText.enabled = false;
    }

    private void ShowUI()
    {
        targetImage.enabled = true;
        targetNameText.enabled = true;
        targetDescriptionText.enabled = true;
    }
}