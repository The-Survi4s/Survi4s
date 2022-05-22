using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private int _maxHitPoint;
    [SerializeField] private float _defaultMoveSpeed;
    public event Action OnPlayerDead;
    public event Action OnPlayerRevived;
    private bool _actionInvoked = false;

    [SerializeField] private float _hitPoint;
    public float hitPoint
    {
        get => _hitPoint;
        set
        {
            _hitPoint = value;

            if (_hitPoint <= 0 && !_actionInvoked)
            {
                OnPlayerDead?.Invoke();
                _hitPoint = 0;
                _actionInvoked = true;
            }

            if (_actionInvoked && _hitPoint > 0)
            {
                OnPlayerRevived?.Invoke();
                _actionInvoked = false;
            }

            if (_hitPoint > _maxHitPoint)
            {
                _hitPoint = _maxHitPoint;
            }
        }
    }

    public float moveSpeed { get; private set; }

    private void Start()
    {
        hitPoint = _maxHitPoint;
        moveSpeed = _defaultMoveSpeed;
    }

    public void CorrectDeadPosition(Vector2 pos)
    {
        transform.position = pos;
    }
    public bool isDead => _hitPoint <= 0;
}
