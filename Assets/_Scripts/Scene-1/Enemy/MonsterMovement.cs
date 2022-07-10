using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Class that handles <see cref="Monster"/>'s movement. 
/// Requires <see cref="NavMeshAgent"/> component attached. 
/// </summary>
[RequireComponent(typeof(NavMeshAgent), typeof(Monster))]
public class MonsterMovement : MonoBehaviour
{
    /// <summary>
    /// The <see cref="Monster"/> component to get stats from
    /// </summary>
    private Monster _owner;
    /// <summary>
    /// The agent used to move things around
    /// </summary>
    private NavMeshAgent _agent;
    [SerializeField] private Target _currentTarget;
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

    [Header("Debug")]
    public NavMeshPathStatus pathStatus;
    public float remainingDistance;
    public Vector3 desVel;

    void Start()
    {
        _owner = GetComponent<Monster>();
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateUpAxis = false;
        _agent.updateRotation = false;

        _currentTargetPreviousPos = Vector3.zero;
        _statuePos = TilemapManager.Instance.statue.transform.position;

        stationaryTime = 0;
        _agent.SetDestination(_statuePos);
    }

    // Update is called once per frame
    void Update()
    {
        if (_owner.isDead)
        {
            _agent.speed = 0;
            return;
        }
        SetStat();
        DecideTarget();
        UpdateStationaryTime();
        RecalculatePath();
        remainingDistance = _agent.remainingDistance;
        pathStatus = _agent.pathStatus; 
        desVel = _agent.desiredVelocity;
    }

    /// <summary>
    /// Re-calculate path only when target has moved at least <see cref="maxDistanceDifference"/> long
    /// </summary>
    private void RecalculatePath()
    {
        var targetPos = _currentTargetPos + _targetOffset;
        if (Vector3.Distance(_currentTargetPreviousPos, targetPos) > maxDistanceDifference || stationaryTime > 10)
        {
            _agent.SetDestination(targetPos);
            _currentTargetPreviousPos = targetPos;
        }
    }

    /// <summary>
    /// Updates how long the agent has been idle
    /// </summary>
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
        if (stationaryTime < 3) return;
        _currentTarget = _owner.setting.priority;
        switch (_currentTarget)
        {
            case Target.Wall:
                _currentTargetPos = _targetWallPos;
                break;
            case Target.Player:
                _currentTargetPos = _targetPlayerPos;
                break;
            case Target.Statue:
                _currentTargetPos = _statuePos;
                break;
            default:
                _currentTargetPos = _statuePos;
                _currentTarget = Target.Statue;
                break;
        }
    }

    private void SetAlternativeTarget()
    {
        if (stationaryTime > 5) TargetWallInstead();
    }

    private void WhenDistracted()
    {
        if (Vector2.Distance(_targetPlayerPos, transform.position) < _owner.setting.detectionRange)
        {
            if (_owner.setting.MethodOf(Target.Player) != Monster.TargetMethod.DontAttack)
            {
                _currentTargetPos = _targetPlayerPos;
                _currentTarget = Target.Player;
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
        _owner.RequestNewTargetWall();
        _targetWallPos = _owner.targetWall.transform.position;
        if (_owner.setting.MethodOf(Target.Wall) != Monster.TargetMethod.DontAttack)
        {
            _currentTargetPos = _targetWallPos;
            _currentTarget = Target.Wall;
        }
    }

    public void SetOffset(Vector3 offset, float zRotation)
    {
        _targetOffset = Quaternion.Euler(new Vector3(0, 0, zRotation)) * offset;
    }

    /// <summary>
    /// Updates agent stat and target candidates' position. Stats provided by <see cref="_owner"/>
    /// </summary>
    private void SetStat()
    {
        _agent.speed = _owner.currentStat.movSpd;
        _agent.acceleration = _owner.currentStat.acceleration;
        _agent.stoppingDistance = _owner.setting.minRange;
        _agent.angularSpeed = _owner.currentStat.rotSpd;

        if (!_owner.targetWall) _owner.RequestNewTargetWall();
        _targetWallPos = _owner.targetWall.transform.position;

        if (_owner.targetPlayer && !_owner.targetPlayer.isDead) _targetPlayerPos = _owner.targetPlayer.transform.position;
        else _targetPlayerPos = _statuePos;
    }

    private void OnDrawGizmosSelected()
    {
        if (_agent && _agent.hasPath)
        {
            Vector3 prevPos = transform.position;
            foreach (var c in _agent.path.corners)
            {
                Gizmos.DrawLine(prevPos, c);
                prevPos = c;
            } 
        }
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_targetWallPos, 5);
        Gizmos.DrawWireSphere(_targetPlayerPos, 5);
        Gizmos.DrawSphere(_currentTargetPos, 1);
    }

    [ContextMenu(nameof(ForceSetDestination))]
    public void ForceSetDestination()
    {
        _agent.SetDestination(_currentTargetPos);
    }
}
