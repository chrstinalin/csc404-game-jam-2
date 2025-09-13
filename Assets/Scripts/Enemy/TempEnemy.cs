using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class TempEnemy : MonoBehaviour
{
    [SerializeField] private List<Transform> _wayPoints;
    private NavMeshAgent _agent;
    private int _currentIndex = 0;
    private bool _reversedPath = false;
    private enum AIState
    {
        Walk,
        Attack,
        Chase
    }

    [SerializeField] private AIState _currentState;
    private bool _isAttacking = false;
    [SerializeField] private float _chaseDistance = 10f;
    public Transform _player;

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        if (_agent != null && _wayPoints.Count > 0)
        {
            _agent.destination = _wayPoints[_currentIndex].position;
        }
    }

    private void Update()
    {
        if (_player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, _player.position);
            if (distanceToPlayer <= _chaseDistance)
            {
                _currentState = AIState.Chase;
            }
            else if (_currentState == AIState.Chase)
            {
                _currentState = AIState.Walk;
                if (_wayPoints.Count > 0)
                    _agent.SetDestination(_wayPoints[_currentIndex].position);
            }
        }

        switch (_currentState)
        {
            case AIState.Walk:
                CalculateAIMovement();
                break;
            case AIState.Attack:
                Debug.Log("Attacking...");
                if (!_isAttacking)
                {
                    StartCoroutine(AttackRoutine());
                    _isAttacking = true;
                }
                break;
            case AIState.Chase:
                ChasePlayer();
                break;
        }
    }

    void CalculateAIMovement()
    {
        if (_agent.remainingDistance < 1f && _wayPoints.Count > 0)
        {
            if (!_reversedPath)
            {
                MoveForward();
            }
            else
            {
                MoveReverse();
            }
            _agent.SetDestination(_wayPoints[_currentIndex].position);
            _currentState = AIState.Attack;
        }
    }

    void MoveForward()
    {
        if (_currentIndex == _wayPoints.Count - 1)
        {
            _reversedPath = true;
            _currentIndex--;
        }
        else
        {
            _currentIndex++;
        }
    }

    void MoveReverse()
    {
        if (_currentIndex == 0)
        {
            _reversedPath = false;
            _currentIndex++;
        }
        else
        {
            _currentIndex--;
        }
    }

    void ChasePlayer()
    {
        if (_player != null)
        {
            _agent.SetDestination(_player.position);
        }
    }

    IEnumerator AttackRoutine()
    {
        _agent.isStopped = true;
        yield return new WaitForSeconds(2f);
        _agent.isStopped = false;
        _currentState = AIState.Walk;
        _isAttacking = false;
    }
}