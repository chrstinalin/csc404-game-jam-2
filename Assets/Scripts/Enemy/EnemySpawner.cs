using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval = 5f;
    private int maxEnemies = 3;
    private float spawnRadius = 2.5f;

    [Header("Optional Settings")]
    private bool spawnOnStart = true;
    private bool limitTotalSpawns = false;
    private int totalSpawnLimit = 50;

    private float spawnTimer;
    private int enemiesSpawned = 0;
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    void Start()
    {
        spawnTimer = spawnOnStart ? 0f : spawnInterval;
    }

    void Update()
    {
        spawnedEnemies.RemoveAll(enemy => enemy == null);
        
        spawnTimer -= Time.deltaTime;
        
        if (spawnTimer <= 0f)
        {
            if (CanSpawn())
            {
                SpawnEnemy();
                spawnTimer = spawnInterval;
            }
            else
            {
                spawnTimer = 1f;
            }
        }
    }

    private bool CanSpawn()
    {
        if (spawnedEnemies.Count >= maxEnemies)
        {
            return false;
        }
        
        if (limitTotalSpawns && enemiesSpawned >= totalSpawnLimit)
        {
            return false;
        }
        
        return true;
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy prefab not assigned to spawner!");
            return;
        }
        
        Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
        randomOffset.y = 0;
        Vector3 spawnPosition = transform.position + randomOffset;

        GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        spawnedEnemies.Add(newEnemy);
        enemiesSpawned++;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}