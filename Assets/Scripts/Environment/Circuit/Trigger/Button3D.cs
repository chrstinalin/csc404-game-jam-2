using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class Button3D : TriggerAbstract
{
    public List<GameObject> TriggerObjects = new List<GameObject>();
    public bool isInvisible;
    [SerializeField] private GameObject unpressedModel;
    [SerializeField] private GameObject pressedModel;

    private HashSet<GameObject> activeColliders = new HashSet<GameObject>();

    private void Awake()
    {
        if (!unpressedModel || !pressedModel)
            throw new MissingReferenceException(
                $"{name}: Both unpressed and pressed models must be assigned.");

        var col = GetComponent<Collider>();
        col.isTrigger = true;

        UpdateVisuals();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (TriggerObjects.Contains(other.gameObject))
        {
            activeColliders.Add(other.gameObject);

            if (!IsActive && activeColliders.Count > 0)
            {
                Activate();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (TriggerObjects.Contains(other.gameObject))
        {
            activeColliders.Remove(other.gameObject);

            if (IsActive && activeColliders.Count == 0)
            {
                Deactivate();
            }
        }
    }

    public override void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        UpdateVisuals();
        if (!isInvisible)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonPressSFX, transform.position, 5f);
        }
        
    }

    public override void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        unpressedModel.SetActive(!IsActive);
        pressedModel.SetActive(IsActive);
    }
}
