using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrolNonHostile : MonoBehaviour
{
    private NavMeshAgent Agent;

    [SerializeField] private GameObject[] WayPoints;
    private int CurrentIndex = 0;

    private void Start()
    {
        Agent = GetComponent<NavMeshAgent>();

        if (WayPoints != null && WayPoints.Length > 0)
        {
            Agent.SetDestination(WayPoints[CurrentIndex].transform.position);
        }
    }

    private void Update()
    {
        Patrol();
    }

    void Patrol()
    {
        if (WayPoints == null || WayPoints.Length == 0) return;

        if (Agent.remainingDistance <= Agent.stoppingDistance + 0.1f)
        {
            if (!Agent.pathPending)
            {
                CurrentIndex = (WayPoints.Length == 1) 
                    ? 0 
                    : Random.Range(0, WayPoints.Length);

                Agent.SetDestination(WayPoints[CurrentIndex].transform.position);
            }
        }
    }
}