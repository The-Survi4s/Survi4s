using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterMovement : MonoBehaviour
{
    private Monster _owner;
    private NavMeshAgent _agent;
    [SerializeField] private Transform _currentTarget;
    private Vector3 _currentTargetPreviousPos;
    private float _previousDistance;
    public float stationaryTime { get; private set; }
    public Vector3 velocity => _agent.velocity;
    [SerializeField] private float Tolerance = 0.5f;
    [SerializeField] private float MaxOffset = 1;

    void Start()
    {
        _owner = GetComponent<Monster>();
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateUpAxis = false;
        _agent.updateRotation = false;
        _currentTarget = transform;
        _currentTargetPreviousPos = Vector3.zero;
        stationaryTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        SetStat();
        if (Vector3.Distance(_currentTargetPreviousPos, _currentTarget.position) > MaxOffset)
        {
            _agent.SetDestination(_currentTarget.position);
            _currentTargetPreviousPos = _currentTarget.position;
        }

        if (_agent.remainingDistance < _previousDistance + Tolerance &&
            _agent.remainingDistance > _previousDistance - Tolerance)
        {
            stationaryTime += Time.deltaTime;
        }
        else
        {
            _previousDistance = _agent.remainingDistance;
            stationaryTime = 0;
        }
    }

    public void SetTarget(Transform target)
    {
        _currentTarget = target;
    }

    private void SetStat()
    {
        _agent.speed = _owner.currentStat.movSpd;
        _agent.acceleration = _owner.currentStat.acceleration;
        _agent.stoppingDistance = _owner.setting.minRange;
        _agent.angularSpeed = _owner.currentStat.rotSpd;
    }
}
