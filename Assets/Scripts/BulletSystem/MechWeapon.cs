using System.Collections;
using UnityEngine;
using FMODUnity;

public class MechWeapon : MonoBehaviour
{
    public EventReference chargeSFX;
    public EventReference bulletSFX;
    [SerializeField] private LineRenderer shotLineRenderer;
    private MechAIController aiController;
    public IOffense Owner { get; set; }

    [Header("Configuration")]
    private float _timer;
    private bool isCharging = false;
    private float damage = 1f;
    [SerializeField] private float fireCooldown = 1f;
    [SerializeField] private float chargeTime = 1f;
    [SerializeField] private float shotDisplayTime = 0.25f;

    void Start()
    {
        isCharging = false;
        aiController = MechAIController.Instance;
        Owner = aiController;
    
        if (shotLineRenderer != null) shotLineRenderer.enabled = false;
    }
    void Update()
    {
        _timer += Time.deltaTime;

        if (Owner == null && MechAIController.Instance != null)
        {
            aiController = MechAIController.Instance;
            Owner = aiController;
        }
        bool ownerIsAttacking = Owner != null && Owner.isAttack();
        bool canFire = _timer >= fireCooldown && Owner != null && ownerIsAttacking && !isCharging;

        if (canFire)
        {
            StartCoroutine(ChargeAndFire());
            _timer = 0;
        }
    }

    public virtual void Fire()
    {
        if (aiController == null) return;

        GameObject target = aiController.GetCurrentTarget();
        if (target == null) return;
        if (!target.activeInHierarchy) return;

        DamageReceiver targetDamageReceiver = target.GetComponent<DamageReceiver>();
        if (targetDamageReceiver == null) return;

        Vector3 shootFrom = transform.position;
        Vector3 fireDirection = (target.transform.position - shootFrom).normalized;
        RaycastHit hit;

        if (Physics.Raycast(shootFrom, fireDirection, out hit, Mathf.Infinity))
        {
            bool hitTarget = hit.transform == target.transform || hit.transform.IsChildOf(target.transform) 
                                                               || target.transform.IsChildOf(hit.transform);
            if (hitTarget)
            {
                AudioManager.Instance.PlaySFX(bulletSFX);
                if (targetDamageReceiver != null)
                {
                    GameObject damageSource = PlayerMech.Instance != null 
                        ? PlayerMech.Instance.gameObject : gameObject;
                    targetDamageReceiver.ReceiveDamage((int)damage, damageSource);
                }
                StartCoroutine(ShowShot(shootFrom, hit.point));
            }
        }
    }
    
    private IEnumerator ChargeAndFire()
    {
        isCharging = true;
        AudioManager.Instance.PlaySFX(chargeSFX);
        yield return new WaitForSeconds(chargeTime);
    
        Fire();
        isCharging = false;
    }

    private IEnumerator ShowShot(Vector3 start, Vector3 end)
    {
        if (shotLineRenderer != null)
        {
            shotLineRenderer.enabled = true;
            shotLineRenderer.SetPosition(0, start);
            shotLineRenderer.SetPosition(1, end);

            yield return new WaitForSeconds(shotDisplayTime);
            shotLineRenderer.enabled = false;
        }
    }
}