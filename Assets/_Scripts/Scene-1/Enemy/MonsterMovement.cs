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
    public float maxStationaryTime = 5;
    private Vector3 _statuePos;
    private Vector3 _targetPlayerPos;
    private Vector3 _targetWallPos;
    public Vector3 velocity => _agent.velocity;
    [SerializeField] private float Tolerance = 0.5f;
    [SerializeField] private float MaxOffset = 1;

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
        if (Vector3.Distance(_currentTargetPreviousPos, _currentTargetPos) > MaxOffset)
        {
            _agent.SetDestination(_currentTargetPos);
            _currentTargetPreviousPos = _currentTargetPos;
        }
    }

    private void UpdateStationaryTime()
    {
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

    private void DecideTarget()
    {
        _currentTarget = _owner.setting.priority;
        switch (_owner.setting.priority)
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

        if (Vector3.Distance(_targetPlayerPos, transform.position) < _owner.setting.detectionRange)
        {
            if (_owner.setting.MethodOf(Monster.Target.Player) != Monster.TargetMethod.DontAttack)
            {
                _currentTargetPos = _targetPlayerPos;
                _currentTarget = Monster.Target.Player;
            }
        }
    }

    private void TargetWallInstead()
    {
        if (_owner.setting.MethodOf(Monster.Target.Wall) != Monster.TargetMethod.DontAttack)
        {
            if (!_owner.targetWall) _owner.RequestNewTargetWall();
            _targetWallPos = _owner.targetWall.transform.position;
            _currentTargetPos = _targetWallPos;
            _currentTarget = Monster.Target.Wall;
        }
    }

    private void SetStat()
    {
        _agent.speed = _owner.currentStat.movSpd;
        _agent.acceleration = _owner.currentStat.acceleration;
        _agent.stoppingDistance = _owner.setting.minRange;
        _agent.angularSpeed = _owner.currentStat.rotSpd;

        if (!_owner.nearestPlayer.isDead) _targetPlayerPos = _owner.nearestPlayer.transform.position;
        else _targetPlayerPos = _statuePos;
    }
}
