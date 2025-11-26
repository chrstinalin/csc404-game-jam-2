using System;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Mouse Health UI
    [NonSerialized] private Health MouseHealth;
    [NonSerialized] private Image[] HealthPoints;

    // Mech Health UI
    [NonSerialized] private Health MechHealth;
    [NonSerialized] private Image HealthFront;
    [NonSerialized] private Image HealthBack;
    [NonSerialized] private float lerpTimer;
    [NonSerialized] private float DAMAGE_LERP = 5f;
    [NonSerialized] private float CHIP_LERP = 50f;

    private GameObject mouseControlsUI;
    private GameObject mechControlsUI;

    private void Start()
    {
        GameObject HealthPointContainer = GameObject.FindGameObjectWithTag("MouseHealthPointContainer");
        GameObject _HealthFront = GameObject.FindGameObjectWithTag("MechHealthFront");
        GameObject _HealthBack = GameObject.FindGameObjectWithTag("MechHealthBack");
        mouseControlsUI = GameObject.FindGameObjectWithTag("MouseControls");
        mechControlsUI = GameObject.FindGameObjectWithTag("MechControls");

        if(!HealthPointContainer || !_HealthFront || !_HealthBack)
        {
            Debug.LogError("UI elements not found.");
            return;
        }

        // MOUSE
        HealthPoints = HealthPointContainer.GetComponentsInChildren<Image>();

        if (PlayerMouse.Instance != null)
        {
            MouseHealth = PlayerMouse.Instance.GetComponent<Health>();
            if (MouseHealth != null)
            {
                MouseHealth.onHealthChanged.AddListener(OnMouseHealthChanged);
            }
        }

        // MECH
        HealthFront = _HealthFront.GetComponent<Image>();
        HealthBack = _HealthBack.GetComponent<Image>();

        if (PlayerMech.Instance != null)
        {
            MechHealth = PlayerMech.Instance.GetComponent<Health>();
            if (MechHealth != null)
            {
                MechHealth.onMaxedHealth.AddListener(OnMechMaxedHealth);
                MechHealth.onHealthChanged.AddListener(OnMechHealthChanged);
            }
        }
    }

    private void Update()
    {
        updateMechHealthUI();
    }

    private void updateMechHealthUI()
    {
        if(!HealthFront || !HealthBack || !MechHealth) return;

        float fillA = HealthFront.fillAmount;
        float fillB = HealthBack.fillAmount;
        float hFraction = (float) MechHealth.GetCurrHealth() / MechHealth.GetMaxHealth();
        if (fillB > hFraction)
        {
            HealthBack.color = Color.red;
            HealthFront.fillAmount = hFraction;

            lerpTimer += Time.deltaTime;

            HealthFront.fillAmount = Mathf.Lerp(fillA, hFraction, lerpTimer / DAMAGE_LERP);
            HealthBack.fillAmount = Mathf.Lerp(fillB, hFraction, lerpTimer / CHIP_LERP);
        }
        else if (fillA < hFraction)
        {
            HealthBack.color = Color.green;
            HealthBack.fillAmount = hFraction;
            lerpTimer += Time.deltaTime;

            HealthFront.fillAmount = Mathf.Lerp(fillA, hFraction, lerpTimer / DAMAGE_LERP);
            HealthBack.fillAmount = Mathf.Lerp(fillB, hFraction, lerpTimer / CHIP_LERP);
        }
    }

    public void OnMechHealthChanged(int damage) => lerpTimer = 0;

    private void OnMechMaxedHealth(int HealthChange)
    {
        if (HealthFront != null) HealthFront.fillAmount = 1f;
        if (HealthBack != null) HealthBack.fillAmount = 1f;
    }

    public void OnMouseHealthChanged(int damage)
    {
        if (MouseHealth == null || HealthPoints == null) return;
        
        for (int i = 0; i < MouseHealth.GetMaxHealth(); i++)
        {
            if (i < HealthPoints.Length)
            {
                HealthPoints[i].enabled = i <= MouseHealth.GetCurrHealth() - 1;
            }
        }
    }

    public void SetActiveCharacterUI(bool isMouseActive)
    {
        if (mouseControlsUI != null)
            mouseControlsUI.SetActive(isMouseActive);

        if (mechControlsUI != null)
            mechControlsUI.SetActive(!isMouseActive);
    }

}