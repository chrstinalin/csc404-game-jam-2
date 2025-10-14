using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class Button : TriggerAbstract
{
    public List<GameObject> TriggerObjects = new List<GameObject>();
    [SerializeField] private GameObject unpressedModel;
    [SerializeField] private GameObject pressedModel;

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
        Debug.Log("" + other.gameObject.name);
        if (TriggerObjects.Contains(other.gameObject))
        {
            Debug.Log("Triggered");
            Activate();
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (TriggerObjects.Contains(other.gameObject))
        {
            Deactivate();
        }
    }

    public override void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        UpdateVisuals();
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
