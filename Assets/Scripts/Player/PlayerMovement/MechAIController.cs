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
    
    private Animator Animator;
    private MechWeapon MechWeapon;

    private Vector3 lastPosition;
    private float stuckCheckTimer = 0f;

    void Awake()
    {
        Instance = this;
        Agent = GetComponent<NavMeshAgent>();
        Animator = GetComponentInChildren<Animator>();
        MechWeapon = GetComponentInChildren<MechWeapon>(true);
        if (Agent != null)
        {
            Agent.updateRotation = true;
        }
    }

    void Start()
    {
        CurrentState = AIState.Idle;
        lastPosition = transform.position;
        
        if (PlayerMarker.Instance != null)
        {
            PlayerMarker.Instance.OnTargetChanged += SetTarget;
        }
    }

    void Update()
    {
        if (Agent == null) return;

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

        Vector3 targetPos = Target.transform.position;
        Vector3 directionToTarget = targetPos - transform.position;
        directionToTarget.y = 0;
        float distance = directionToTarget.magnitude;

        // Ground navigation
        if (Target.name == "GroundTarget")
        {
            // Stuck/blocked check
            stuckCheckTimer += Time.deltaTime;
            if (stuckCheckTimer >= Config.STUCK_CHECK_INTERVAL)
            {
                Vector2 currentPosXZ = new Vector2(transform.position.x, transform.position.z);
                Vector2 lastPosXZ = new Vector2(lastPosition.x, lastPosition.z);
                float movedDistance = Vector2.Distance(currentPosXZ, lastPosXZ);
                
                if (movedDistance < Config.STUCK_THRESHOLD && !Agent.isStopped)
                {
                    Destroy(Target);
                    Target = null;
                    if (PlayerMarker.Instance != null) PlayerMarker.Instance.Target = null;
                    CurrentState = AIState.Idle;
                    AttackActive = false;
                    Agent.isStopped = true;
                    Agent.ResetPath();
                    UpdateAnimator();
                    return;
                }
                
                lastPosition = transform.position;
                stuckCheckTimer = 0f;
            }
            
            if (distance <= Config.MIN_AI_DISTANCE)
            {
                Destroy(Target);
                Target = null;
                if (PlayerMarker.Instance != null) PlayerMarker.Instance.Target = null;
                CurrentState = AIState.Idle;
                AttackActive = false;
                Agent.isStopped = true;
                Agent.ResetPath();
            }
            else
            {
                CurrentState = AIState.Walk;
                AttackActive = false;
                Agent.isStopped = false;
                Vector3 destination = targetPos;
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(targetPos, out navHit, 2.0f, NavMesh.AllAreas))
                {
                    destination = navHit.position;
                }
                Agent.SetDestination(destination);
            }
            UpdateAnimator();
            return;
        }

        bool isPlayerMouse = Target == PlayerMouse.Instance?.gameObject;
        bool isPlayerMarker = Target == PlayerMarker.Instance?.gameObject;
        bool isLockOnSelectable = Target.GetComponent<LockOnSelectable>() != null;
        bool isEnemy = Target.GetComponent<DamageReceiver>() != null && !isPlayerMouse && !isPlayerMarker;

        if (isEnemy)
        {
            if (distance <= Config.ATTACK_RANGE)
            {
                CurrentState = AIState.Idle;
                Agent.isStopped = true;
                Agent.ResetPath();
                AttackActive = false;

                if (directionToTarget != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
                }
                if (Input.GetButtonDown("Interact") && MechWeapon != null)
                {
                    MechWeapon.Fire();
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
        else if (isPlayerMarker)
        {
            GameObject markerTarget = PlayerMarker.Instance?.Target;
            if (markerTarget != null && markerTarget != gameObject)
            {
                bool markerTargetIsEnemy = markerTarget.GetComponent<DamageReceiver>() != null;
                
                if (markerTargetIsEnemy)
                {
                    Vector3 enemyPos = markerTarget.transform.position;
                    float distToEnemy = Vector3.Distance(transform.position, enemyPos);
                    
                    if (distToEnemy <= Config.ATTACK_RANGE)
                    {
                        CurrentState = AIState.Idle;
                        Agent.isStopped = true;
                        Agent.ResetPath();
                        AttackActive = false;
                        
                        Vector3 dirToEnemy = enemyPos - transform.position;
                        dirToEnemy.y = 0;
                        if (dirToEnemy != Vector3.zero)
                        {
                            Quaternion targetRotation = Quaternion.LookRotation(dirToEnemy);
                            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
                        }

                        if (Input.GetButtonDown("Interact") && MechWeapon != null)
                        {
                            MechWeapon.Fire();
                        }
                    }
                    else
                    {
                        CurrentState = AIState.Walk;
                        Agent.isStopped = false;
                        AttackActive = false;
                        Agent.SetDestination(enemyPos);
                    }
                }
                else
                {
                    CurrentState = AIState.Walk;
                    AttackActive = false;
                    Agent.isStopped = false;
                    Agent.SetDestination(markerTarget.transform.position);
                }
            }
            else
            {
                CurrentState = AIState.Idle;
                AttackActive = false;
                Agent.isStopped = true;
                Agent.ResetPath();
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
        else if (isPlayerMouse)
        {
            AttackActive = false;
            if (Input.GetButton("SummonMecha"))
            {
                if (distance > Config.MIN_AI_DISTANCE)
                {
                    CurrentState = AIState.Walk;
                    Agent.isStopped = false;
                    Agent.SetDestination(targetPos);
                }
                else
                {
                    CurrentState = AIState.Idle;
                    Agent.isStopped = true;
                    Agent.ResetPath();
                }
            }
            else
            {
                CurrentState = AIState.Idle;
                Agent.isStopped = true;
                Agent.ResetPath();
            }
        }
        else
        {
            CurrentState = AIState.Walk;
            AttackActive = false;
            Agent.isStopped = false;
            Agent.SetDestination(targetPos);
        }
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        if (Animator == null) return;
        bool isWalking = CurrentState == AIState.Walk && Agent != null && !Agent.isStopped;
        Animator.SetBool("isRunning", isWalking);
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