using System;
using UnityEngine;
using UnityEngine.AI;

public class MechAIController : MonoBehaviour, IOffense
{
    [NonSerialized] public static MechAIController Instance;

    public NavMeshAgent Agent;
    public GameObject Target;
    private AIState CurrentState;

    private bool AttackActive = false;
    [SerializeField] private float attackRange = 20f;
    
    private GameObject lastAttacker;
    private float lastAttackTime;
    private float retaliationMemoryDuration = 5f;

    void Awake()
    {
        Instance = this;
        Agent = GetComponent<NavMeshAgent>();
        if (Agent != null)
        {
            Agent.updateRotation = true;
        }
    }

    void Start()
    {
        CurrentState = AIState.Idle;
        if (PlayerMarker.Instance != null)
        {
            PlayerMarker.Instance.OnTargetChanged += SetTarget;
        }
    }

    public void OnAttackedBy(GameObject attacker)
    {
        if (attacker == null) return;
        
        Debug.Log("MechAIController.OnAttackedBy called with attacker: " + attacker.name);
        
        lastAttacker = attacker;
        lastAttackTime = Time.time;
        
        SetTarget(attacker);
    }

    void Update()
    {
        if (Agent == null) return;

        if (lastAttacker != null && Time.time - lastAttackTime < retaliationMemoryDuration)
        {
            Health attackerHealth = lastAttacker.GetComponent<Health>();
            if (attackerHealth == null || attackerHealth.GetCurrHealth() <= 0)
            {
                lastAttacker = null;
                if (Target == lastAttacker)
                {
                    SetTarget(null);
                }
            }
        }
        else if (lastAttacker != null && Time.time - lastAttackTime >= retaliationMemoryDuration)
        {
            lastAttacker = null;
        }

        if (Target == null)
        {
            AttackActive = false;
            if (CurrentState == AIState.Attack || CurrentState == AIState.Walk)
            {
                Agent.isStopped = true;
                Agent.ResetPath();
                CurrentState = AIState.Idle;
            }
            return;
        }

        // Ignore ground targets
        if (Target.name == "GroundTarget")
        {
            AttackActive = false;
            Agent.isStopped = true;
            Agent.ResetPath();
            CurrentState = AIState.Idle;
            return;
        }

        Vector3 targetPos = Target.transform.position;
        Vector3 directionToTarget = targetPos - transform.position;
        directionToTarget.y = 0;
        float distance = directionToTarget.magnitude;

        bool isPlayerMouse = Target == PlayerMouse.Instance?.gameObject;
        bool isLockOnSelectable = Target.GetComponent<LockOnSelectable>() != null;
        bool isEnemy = Target.GetComponent<DamageReceiver>() != null && !isPlayerMouse;

        if (isEnemy)
        {
            if (distance <= attackRange)
            {
                CurrentState = AIState.Attack;
                Agent.isStopped = true;
                Agent.ResetPath();
                AttackActive = true;

                if (directionToTarget != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
                }
            }
            else
            {
                CurrentState = AIState.Walk;
                Agent.isStopped = false;
                AttackActive = false;
                Agent.SetDestination(targetPos);
            }
        }
        else if (isLockOnSelectable)
        {
            CurrentState = (distance > Config.MIN_AI_DISTANCE) ? AIState.Walk : AIState.Idle;
            AttackActive = false;
            
            if (CurrentState == AIState.Idle)
            {
                Agent.isStopped = true;
                Agent.ResetPath();
            }
            else
            {
                Agent.isStopped = false;
                Vector3 destination = targetPos - directionToTarget.normalized * Config.MIN_AI_DISTANCE;
                Agent.SetDestination(destination);
            }
        }
        else
        {
            CurrentState = AIState.Walk;
            AttackActive = false;
            Agent.isStopped = false;
            Agent.SetDestination(targetPos);
        }
    }

    public void SetTarget(GameObject NewTarget)
    {    
        if (Target != null)
        {
            Health oldHealth = Target.GetComponent<Health>();
            if (oldHealth != null)
            {
                oldHealth.onDeath.RemoveListener(HandleTargetDeath);
            }
        }
    
        bool targetChanged = Target != NewTarget;
        Target = NewTarget;
        
        if (Target != null)
        {
            if (targetChanged)
            {
                Health newHealth = Target.GetComponent<Health>();
                if (newHealth != null)
                {
                    newHealth.onDeath.AddListener(HandleTargetDeath);
                }
            }

            if (targetChanged)
            {
                AttackActive = false;
                CurrentState = AIState.Walk;
            }

            if (Agent != null)
            {
                Agent.isStopped = false;
                Agent.SetDestination(Target.transform.position);
            }
        }
        else
        {
            CurrentState = AIState.Idle;
            AttackActive = false;

            if (Agent != null)
            {
                Agent.isStopped = true;
                Agent.ResetPath();
            }
        }
    }

    private void HandleTargetDeath()
    {
        if (Target != null)
        {
            Health deadHealth = Target.GetComponent<Health>();
            if (deadHealth != null)
            {
                deadHealth.onDeath.RemoveListener(HandleTargetDeath);
            }
        }
        Target = null;
        AttackActive = false;
        CurrentState = AIState.Idle;

        if (Agent != null)
        {
            Agent.isStopped = true;
            Agent.ResetPath();
        }
    }
    
    public bool isAttack()
    {
        return AttackActive;
    }

    public GameObject GetCurrentTarget()
    {
        return Target;
    }

    void OnDestroy()
    {
        if (Target != null)
        {
            Health health = Target.GetComponent<Health>();
            if (health != null) health.onDeath.RemoveListener(HandleTargetDeath);
        }
        
        if (PlayerMarker.Instance != null) PlayerMarker.Instance.OnTargetChanged -= SetTarget;
    }
}