using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.AI;

public class RespawnManager : MonoBehaviour
{
    private static RespawnManager instance;

    public static RespawnManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<RespawnManager>();

                if (instance == null)
                {
                    GameObject go = new GameObject("RespawnManager");
                    instance = go.AddComponent<RespawnManager>();
                }
            }

            return instance;
        }
    }
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartRespawnCountdown(bool isMech)
    {
        StartCoroutine(RespawnCountdown(isMech));
    }

    private IEnumerator RespawnCountdown(bool isMech)
    {
        GameObject character = isMech ? PlayerMech.Instance?.gameObject : PlayerMouse.Instance?.gameObject;
        if (character == null) { Debug.LogWarning("RespawnManager: character is null for respawn, aborting respawn countdown"); yield break; }
        if (character.TryGetComponent<MechAIController>(out var aiController)) aiController.enabled = false;
        var navAgent = character.GetComponent<NavMeshAgent>();
        if (navAgent != null) navAgent.enabled = false;
        character.SetActive(false);

        if (isMech && !MovementManager.Instance.IsMouseActive) MovementManager.Instance.ToggleMouse(true);
        else if (!isMech && MovementManager.Instance.IsMouseActive) MovementManager.Instance.ToggleMouse(false);

        TMP_Text countdownText = GameObject.FindGameObjectWithTag(isMech ? "MechCountdown" : "MouseCountdown")?.GetComponentInChildren<TMP_Text>();
        CanvasGroup healthBar = GameObject.FindGameObjectWithTag(isMech ? "MechHealthPointContainer" : "MouseHealthPointContainer")?.GetComponentInChildren<CanvasGroup>();

        if (healthBar != null) healthBar.alpha = 0.5f;

        for (int i = 5; i > 0; i--) { if (countdownText != null) countdownText.text = i.ToString("D2"); yield return new WaitForSeconds(1f); }

        if (countdownText != null) countdownText.text = "";
        if (healthBar != null) healthBar.alpha = 1f;

        var spawnPoint = CheckpointManager.Instance.GetCurrentSpawnPoint();
        var spawnPos = spawnPoint.position;
        var spawnRot = spawnPoint.rotation;
        var offset = Vector3.right * 1.5f;

        if (isMech)
        {
            PlayerMech.Instance.transform.SetPositionAndRotation(spawnPos + offset, spawnRot);
            if (PlayerMech.Instance.TryGetComponent<Rigidbody>(out var rb)) rb.linearVelocity = Vector3.zero;
        }
        else
        {
            PlayerMouse.Instance.transform.SetPositionAndRotation(spawnPos - offset, spawnRot);
            if (PlayerMouse.Instance.TryGetComponent<Rigidbody>(out var rb)) rb.linearVelocity = Vector3.zero;
        }

        character.SetActive(true);
        if (character.TryGetComponent<MechAIController>(out var ai2)) ai2.enabled = true;
        var navAgent2 = character.GetComponent<NavMeshAgent>();
        if (navAgent2 != null) navAgent2.enabled = true;

        if (isMech && PlayerMech.Instance?.Health != null) { 
            PlayerMech.Instance.Health.Heal(PlayerMech.Instance.Health.GetMaxHealth());
        } else if (PlayerMouse.Instance?.Health != null) {
            PlayerMouse.Instance.Health.Heal(PlayerMouse.Instance.Health.GetMaxHealth());
        }

    }
}