using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterMovement : MonoBehaviour
{
    private Monster _owner;
    private NavMeshAgent _agent;
    private Transform _currentTarget;
    private Vector3 _currentTargetPreviousPos;
    [SerializeField] private float _maxOffset = 1;

    void Start()
    {
        _owner = GetComponent<Monster>();
        _agent = GetComponent<NavMeshAgent>();
        _currentTarget = transform;
        _currentTargetPreviousPos = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        SetStat();
        if (Vector3.Distance(_currentTargetPreviousPos, _currentTarget.position) > _maxOffset)
        {
            _agent.SetDestination(_currentTarget.position);
            _currentTargetPreviousPos = _currentTarget.position;
        }
    }

    public void UpdateTarget(Transform target)
    {
        _currentTarget = target;
    }

    private void SetStat()
    {
        _agent.speed = _owner.currentStat.movSpd;
    }
}
