using UnityEngine;
using System.Collections.Generic;
using FMODUnity;

[RequireComponent(typeof(Collider))]
public class Button3D : TriggerAbstract
{
    public List<GameObject> TriggerObjects = new List<GameObject>();
    public List<TriggerAbstract> SoundTriggerDependencies = new List<TriggerAbstract>();

    [SerializeField] private EventReference buttonPressSFX;

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

        // Only play SFX if all dependencies are active
        bool allDependenciesActive = true;
        foreach (var dep in SoundTriggerDependencies)
        {
            if (dep == null || !dep.IsActive)
            {
                allDependenciesActive = false;
                break;
            }
        }

        if (allDependenciesActive && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(buttonPressSFX, transform.position, 5f);
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
