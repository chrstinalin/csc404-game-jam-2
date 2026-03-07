using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

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
    private Vector3 directionToTarget;
    private float stuckCheckTimer = 0f;

    void Awake()
    {
        Instance = this;
        Agent = GetComponent<NavMeshAgent>();
        Animator = GetComponentInChildren<Animator>();
        MechWeapon = GetComponentInChildren<MechWeapon>(true);

        if (Agent != null)
            Agent.updateRotation = true;
    }

    void Start()
    {
        CurrentState = AIState.Idle;
        lastPosition = transform.position;

        if (LockOnManager.Instance != null)
            LockOnManager.Instance.OnLockOnInteract += HandleLockOnInteract;
    }

    private void HandleLockOnInteract(GameObject target)
    {
        if (target == null) return;

        float distance = directionToTarget.magnitude;

        // Only fire if it’s an enemy
        if (
            target.GetComponent<DamageReceiver>() != null
            && target != PlayerMouse.Instance.gameObject 
            && (distance <= Config.ATTACK_RANGE)
        )
        {
            MechWeapon weapon = GetComponentInChildren<MechWeapon>();
            if (weapon != null)
            {
                weapon.Fire();
            }
        }
    }
    void Update()
    {
        if (!gameObject.activeSelf || Agent == null) return;

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
        directionToTarget = targetPos - transform.position;
        directionToTarget.y = 0;

        bool isPlayerMouse = Target == PlayerMouse.Instance?.gameObject;
        float distance = directionToTarget.magnitude;

        if (isPlayerMouse)
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
            CurrentState = AIState.Idle;
            AttackActive = false;
            Agent.isStopped = true;
            Agent.ResetPath();
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
        if (!gameObject.activeSelf) return;

        if (Target != null)
        {
            Health oldHealth = Target.GetComponent<Health>();
            if (oldHealth != null)
                oldHealth.onDeath.RemoveListener(HandleTargetDeath);
        }

        bool targetChanged = Target != NewTarget;
        Target = NewTarget;

        if (Target != null && targetChanged)
        {
            Health newHealth = Target.GetComponent<Health>();
            if (newHealth != null)
                newHealth.onDeath.AddListener(HandleTargetDeath);

            AttackActive = false;
            CurrentState = AIState.Walk;

            if (Agent != null)
            {
                Agent.isStopped = false;
                Agent.SetDestination(Target.transform.position);
            }
        }
        else if (Target == null)
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
    public bool isAttack() => AttackActive;
    public GameObject GetCurrentTarget() => Target;

    void OnDestroy()
    {
        if (Target != null)
        {
            Health health = Target.GetComponent<Health>();
            if (health != null)
                health.onDeath.RemoveListener(HandleTargetDeath);
        }

        if (LockOnManager.Instance != null)
            LockOnManager.Instance.OnLockOnInteract -= HandleLockOnInteract;
    }
}