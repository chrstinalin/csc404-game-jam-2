using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval = 5f;
    private int maxEnemies = 3;
    [SerializeField] private SpawnMode spawnMode = SpawnMode.RandomRadius;
    
    [Header("RandomRadius Settings")]
    [SerializeField] private float spawnRadius = 2.5f;  // RandomRadius only
    
    [Header("Directional Settings")]
    [SerializeField] private Vector3 spawnDirection = Vector3.forward;
    [SerializeField] private float spawnDistance = 1f;
    [SerializeField] private float spawnSpread = 2f;
    
    private bool spawnOnStart = true;
    private bool limitTotalSpawns = false;
    private int totalSpawnLimit = 50;
    private float spawnTimer;
    private int enemiesSpawned = 0;
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    
    private enum SpawnMode
    {
        RandomRadius,       // Spawn in a circle
        Directional,        // Spawn in specific direction with offset
        DirectionalLine     // Spawn along a line in a specific direction
    }
    
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
        if (spawnedEnemies.Count >= maxEnemies) return false;
        if (limitTotalSpawns && enemiesSpawned >= totalSpawnLimit) return false;
        
        return true;
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy prefab not assigned to spawner!");
            return;
        }
        
        Vector3 spawnPosition = CalculateSpawnPosition();
        
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(spawnPosition, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
        {
            spawnPosition = hit.position;
        }
        else
        {
            Debug.LogWarning($"No NavMesh found near {spawnPosition}! Enemy may float.");
        }

        GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        spawnedEnemies.Add(newEnemy);
        enemiesSpawned++;
    }


    private Vector3 CalculateSpawnPosition()
    {
        switch (spawnMode)
        {
            case SpawnMode.RandomRadius:
                Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
                randomOffset.y = 0;
                return transform.position + randomOffset;

            case SpawnMode.Directional:
                Vector3 normalizedDir = spawnDirection.normalized;
                Vector3 basePosition = transform.position + normalizedDir * spawnDistance;
                
                Vector3 perpendicular = Vector3.Cross(normalizedDir, Vector3.up).normalized;
                Vector3 spreadOffset = perpendicular * Random.Range(-spawnSpread, spawnSpread);
                
                return basePosition + spreadOffset;

            case SpawnMode.DirectionalLine:
                Vector3 dirNormalized = spawnDirection.normalized;
                return transform.position + dirNormalized * spawnDistance;

            default:
                return transform.position;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (spawnMode == SpawnMode.RandomRadius)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
        else  // Directional modes
        {
            Vector3 normalizedDir = spawnDirection.normalized;
            Vector3 spawnCenter = transform.position + normalizedDir * spawnDistance;
            Vector3 spawnPoint = transform.position + normalizedDir * spawnDistance;
            Vector3 perpendicular = Vector3.Cross(normalizedDir, Vector3.up).normalized;
            Gizmos.color = Color.red;
            
            // Draw arrow
            Gizmos.DrawLine(transform.position, spawnCenter);
            Vector3 arrowSide1 = spawnCenter - normalizedDir * 0.5f + perpendicular * 0.3f;
            Vector3 arrowSide2 = spawnCenter - normalizedDir * 0.5f - perpendicular * 0.3f;
            Gizmos.DrawLine(spawnCenter, arrowSide1);
            Gizmos.DrawLine(spawnCenter, arrowSide2);

            if (spawnMode == SpawnMode.Directional)
            {
                Gizmos.color = Color.yellow;
                Vector3 spreadLeft = spawnCenter - perpendicular * spawnSpread;
                Vector3 spreadRight = spawnCenter + perpendicular * spawnSpread;
                Gizmos.DrawLine(spreadLeft, spreadRight);
                Gizmos.DrawWireCube(spreadLeft, Vector3.one * 0.3f);
                Gizmos.DrawWireCube(spreadRight, Vector3.one * 0.3f);
                Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                for (int i = 0; i <= 10; i++)
                {
                    float t = i / 10f;
                    Vector3 point = Vector3.Lerp(spreadLeft, spreadRight, t);
                    Gizmos.DrawWireSphere(point, 0.2f);
                }
            }
            else
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(spawnPoint, 0.5f);
            }
        }
    }
}