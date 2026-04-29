using System;
using UnityEngine;
using FMODUnity;

public class Turret : MonoBehaviour
{
    [NonSerialized] public DamageReceiver DamageReceiver;
    [NonSerialized] public Health Health;
    [NonSerialized] public ScrapCurrency scrapCurrency;
    [SerializeField] private GameObject scrapPilePrefab;
    [SerializeField] private EventReference DeathSFX;

    void Start()
    {
        DamageReceiver = gameObject.AddComponent<DamageReceiver>();
        Health = gameObject.AddComponent<Health>();
        Health.SetMaxHealth(1);
        Health.onDeath.AddListener(OnDeath);
        DamageReceiver.onTakeDamage.AddListener(TakeDamage);
    }

    public void TakeDamage(int damage)
    {
        Health.TakeDamage(damage);
    }

    void OnDeath()
    {
        AudioManager.Instance.PlaySFX(DeathSFX, transform.position, 1f);

        if (scrapPilePrefab != null)
        {
            Vector3 dropPosition = transform.position;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 100f))
            {
                dropPosition = hit.point;
            }

            GameObject scrapPile = Instantiate(scrapPilePrefab, dropPosition, Quaternion.identity);
            ScrapCurrency scrapComponent = scrapPile.GetComponent<ScrapCurrency>();
            if (scrapComponent != null)
            {
                scrapComponent.Drop(dropPosition);
            }
        }
        Destroy(gameObject, 0.2f);
    }
}