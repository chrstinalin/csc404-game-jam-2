using System;
using System.Collections;
using UnityEngine;

public class PlayerMouse : MonoBehaviour
{
    public static PlayerMouse Instance;

    [NonSerialized] public Health Health;
    [NonSerialized] public DamageReceiver DamageReceiver;
    [NonSerialized] public MouseInventoryManager InventoryManager;

    [NonSerialized] public GameObject GroundCollider;

    private bool isInvulnerable = false;
    private float iFrameDuration = 1.0f;
    private float iFrameTimer = 0f;

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        InventoryManager = gameObject.AddComponent<MouseInventoryManager>();
        DamageReceiver = gameObject.AddComponent<DamageReceiver>();
        GroundCollider = GameObject.FindGameObjectWithTag("MouseGroundCollider");

        Health = GetComponent<Health>();
        if (Health == null)
            Health = gameObject.AddComponent<Health>();

        Health.onDeath.AddListener(OnDeath);
        DamageReceiver.onTakeDamage.AddListener(TakeDamage);
    }

    void Update()
    {
        if (isInvulnerable)
        {
            iFrameTimer -= Time.deltaTime;
            if (iFrameTimer <= 0f)
            {
                isInvulnerable = false;
            }
        }
    }

    public GameObject getActivePlayer()
    {
        if (gameObject.activeInHierarchy) return gameObject;
        else return PlayerMech.Instance != null ? PlayerMech.Instance.gameObject : null;
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable || Health == null) return;
        Health.TakeDamage(damage);
        isInvulnerable = true;
        iFrameTimer = iFrameDuration;
        StartCoroutine(FlashSprite());
    }

    public void OnDeath()
    {
        var checkpointManager = CheckpointManager.Instance;
        if (checkpointManager != null)
        {
            checkpointManager.RespawnCharacters();
        }

        if (Health != null)
        {
            Health.Heal(Health.GetMaxHealth());
        }
    }

    private IEnumerator FlashSprite()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Color[] originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            // originalColors[i] = renderers[i].material.color;
        }

        float flashInterval = 0.2f;
        float elapsed = 0f;
        Color flashColour = Color.red;
        
        while (elapsed < iFrameDuration)
        {
            bool useFlashColour = ((int)(elapsed / flashInterval)) % 2 == 0;
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.color = useFlashColour ? flashColour : originalColors[i];
            }
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }
        
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = originalColors[i];
        }
    }
}