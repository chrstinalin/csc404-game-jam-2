using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrolManager : MonoBehaviour, IOffense
{
    public EnemyVisionAbstractManager VisionManager;
    private NavMeshAgent Agent;

    [SerializeField] private GameObject[] WayPoints;
    private GameObject ChaseEntity;
    private bool AttackActive = false;
    private int CurrentIndex = 0;
    
    [SerializeField] private float attackDistance = 10f;

    private AIState CurrentState;

    void Start()
    {
        CurrentState = AIState.Walk;

        VisionManager = GetComponent<EnemyVisionAbstractManager>();
        VisionManager.InitVision();

        Agent = GetComponent<NavMeshAgent>();
        if (WayPoints.Length > 0) Agent.destination = WayPoints[CurrentIndex].transform.position;
    }

    void Update()
    {
        VisionManager.UpdateVision();

        ChaseEntity = VisionManager.MouseIsSpotted ? VisionManager.Mouse :
            VisionManager.MechIsSpotted ? VisionManager.Mech :
            null;

        if (ChaseEntity)
        {
            CurrentState = AIState.Chase;
        }
        else if (CurrentState == AIState.Chase)
        {
            CurrentState = AIState.Walk;
            if (WayPoints.Length > 0) Agent.SetDestination(WayPoints[CurrentIndex].transform.position);
        }

        switch (CurrentState)
        {
            case AIState.Walk:
                CalculateAIMovement();
                break;
            case AIState.Attack:
                if (!AttackActive)
                {
                    StartCoroutine(AttackRoutine());
                    AttackActive = true;
                }
                break;
            case AIState.Chase:
                ChasePlayer();
                break;
        }
    }

    public bool isAttack()
    {
        return ChaseEntity != null;
    }

    void CalculateAIMovement()
    {
        if (Agent.remainingDistance < 1f && WayPoints.Length > 0)
        {
            CurrentIndex = (WayPoints.Length == 1) ? 0 : Random.Range(0, WayPoints.Length);
            Agent.SetDestination(WayPoints[CurrentIndex].transform.position);
        }
    }

    void ChasePlayer()
    {
        if (ChaseEntity != null)
        {
            Vector3 targetPos = ChaseEntity.transform.position;
            float distance = Vector3.Distance(transform.position, targetPos);
            
            if (distance <= attackDistance)
            {
                Agent.isStopped = true;
                Agent.ResetPath();
                
                Vector3 lookDirection = (targetPos - transform.position).normalized;
                lookDirection.y = 0;
                if (lookDirection != Vector3.zero) 
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), 
                        Time.deltaTime * 5f);
                }
            }
            else
            {
                Agent.isStopped = false;
                Agent.SetDestination(targetPos);
            }
        }
    }

    IEnumerator AttackRoutine()
    {
        Agent.isStopped = true;
        yield return new WaitForSeconds(2f);
        Agent.isStopped = false;
        CurrentState = AIState.Walk;
        AttackActive = false;
    }
}