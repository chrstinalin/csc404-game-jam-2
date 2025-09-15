using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class EnemyPatrolManager : MonoBehaviour, IOffense
{
    public EnemyVisionAbstractManager VisionManager;
    private NavMeshAgent Agent;

    [SerializeField] private float MinDistanceFromPlayer = 2f;
    [SerializeField] private GameObject[] WayPoints;
    private GameObject ChaseEntity;
    private bool AttackActive = false;
    private int CurrentIndex = 0;

    private AIState CurrentState;
    private enum AIState
    {
        Walk,
        Attack,
        Chase
    }

    void Start()
    {
        WayPoints = GameObject.FindGameObjectsWithTag("PatrolWaypoint");

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
            Vector3 destination = transform.position;
            if (distance > MinDistanceFromPlayer)
            {
                destination = targetPos - (targetPos - transform.position).normalized * MinDistanceFromPlayer;
            }
            Agent.SetDestination(destination);
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

    public bool isAttack()
    {
        return AttackActive;
    }
}
