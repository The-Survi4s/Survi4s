using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterMovement : MonoBehaviour
{
    private Monster _owner;
    private NavMeshAgent _agent;
    [SerializeField] private Monster.Target _currentTarget;
    [SerializeField] private Vector3 _currentTargetPos;
    private Vector3 _currentTargetPreviousPos;
    private float _previousDistance;
    public float stationaryTime;
    [SerializeField] private float _maxStationaryTime;
    private Vector3 _statuePos;
    private Vector3 _targetPlayerPos;
    private Vector3 _targetWallPos;
    public Vector3 velocity => _agent.velocity;
    [SerializeField] private float minMoveDistance = 0.5f;
    [SerializeField] private float maxDistanceDifference = 1;
    private Vector3 _targetOffset;
    private float _distanceToTarget;

    void Start()
    {
        _owner = GetComponent<Monster>();
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateUpAxis = false;
        _agent.updateRotation = false;
        _agent.isStopped = false;

        _currentTargetPreviousPos = Vector3.zero;
        _statuePos = TilemapManager.instance.statue.transform.position;

        stationaryTime = 0;
        _agent.SetDestination(_statuePos);
    }

    // Update is called once per frame
    void Update()
    {
        SetStat();
        DecideTarget();
        UpdateStationaryTime();
        RecalculatePath();
    }

    private void RecalculatePath()
    {
        var targetPos = _currentTargetPos + _targetOffset;
        if (Vector3.Distance(_currentTargetPreviousPos, targetPos) > maxDistanceDifference)
        {
            _agent.SetDestination(targetPos);
            _currentTargetPreviousPos = targetPos;
        }
    }

    private void UpdateStationaryTime()
    {
        if (_agent.remainingDistance < _previousDistance + minMoveDistance &&
            _agent.remainingDistance > _previousDistance - minMoveDistance)
        {
            stationaryTime += Time.deltaTime;
        }
        else
        {
            _previousDistance = _agent.remainingDistance;
            stationaryTime = 0;
        }
    }

    private void DecideTarget()
    {
        SetInitialCurrentTarget();
        SetAlternativeTarget();
        WhenDistracted();
        DoEvasion();
    }

    private void SetInitialCurrentTarget()
    {
        _currentTarget = _owner.setting.priority;
        switch (_currentTarget)
        {
            case Monster.Target.Wall:
                _currentTargetPos = _targetWallPos;
                break;
            case Monster.Target.Player:
                _currentTargetPos = _targetPlayerPos;
                break;
            case Monster.Target.Statue:
                _currentTargetPos = _statuePos;
                break;
            default:
                _currentTargetPos = _statuePos;
                _currentTarget = Monster.Target.Statue;
                break;
        }
    }

    private void SetAlternativeTarget()
    {
        switch (_agent.pathStatus)
        {
            case NavMeshPathStatus.PathComplete:
                break;
            case NavMeshPathStatus.PathPartial:
                //Debug.Log($"{name} path partial");
                TargetWallInstead();
                break;
            case NavMeshPathStatus.PathInvalid:
                Debug.Log($"{name} path invalid");
                TargetWallInstead();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void WhenDistracted()
    {
        if (Vector2.Distance(_targetPlayerPos, transform.position) < _owner.setting.detectionRange)
        {
            if (_owner.setting.MethodOf(Monster.Target.Player) != Monster.TargetMethod.DontAttack)
            {
                _currentTargetPos = _targetPlayerPos;
                _currentTarget = Monster.Target.Player;
            }
        }
    }

    private void DoEvasion()
    {
        _distanceToTarget = Vector2.Distance(transform.position, _currentTargetPos);
        if (_distanceToTarget < _owner.setting.minRange && _owner.setting.doEvasion)
        {
            var oppositeDir = transform.position - _currentTargetPos;
            _currentTargetPos -= 2 * oppositeDir;
        }
    }

    private void TargetWallInstead()
    {
        if (_owner.setting.MethodOf(Monster.Target.Wall) != Monster.TargetMethod.DontAttack)
        {
            _currentTargetPos = _targetWallPos;
            _currentTarget = Monster.Target.Wall;
        }
    }

    public void SetOffset(Vector3 offset, float zRotation)
    {
        _targetOffset = Quaternion.Euler(new Vector3(0, 0, zRotation)) * offset;
    }

    private void SetStat()
    {
        _agent.speed = _owner.currentStat.movSpd;
        _agent.acceleration = _owner.currentStat.acceleration;
        _agent.stoppingDistance = _owner.setting.minRange;
        _agent.angularSpeed = _owner.currentStat.rotSpd;

        if (!_owner.targetWall) _owner.RequestNewTargetWall();
        _targetWallPos = _owner.targetWall.transform.position;

        if (!_owner.nearestPlayer.isDead) _targetPlayerPos = _owner.nearestPlayer.transform.position;
        else _targetPlayerPos = _statuePos;
    }
}
