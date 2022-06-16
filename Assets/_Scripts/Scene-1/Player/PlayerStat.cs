using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    [SerializeField] private int _maxHitPoint;
    [SerializeField] private float _defaultMoveSpeed;

    public event Action<string> PlayerDead;
    public event Action<string> PlayerRevived;
    public bool isInitialized { get; private set; }
    private bool _actionInvoked = false;
    private Player owner;

    [SerializeField] private float _hitPoint;
    public float hitPoint
    {
        get => _hitPoint;
        set
        {
            if (value < _hitPoint && owner.isLocal) GameUIManager.Instance.ShowDamageOverlay(100);
            _hitPoint = value;

            if (_hitPoint <= 0)
            {
                _hitPoint = 0;
                if (!_actionInvoked)
                {
                    PlayerDead?.Invoke(name);
                    _actionInvoked = true;
                }
            }

            if (_actionInvoked && _hitPoint > 0)
            {
                Debug.Log("Player " + name + " is trying to revive");
                PlayerRevived?.Invoke(name);
                _actionInvoked = false;
            }

            if (_hitPoint > _maxHitPoint)
            {
                _hitPoint = _maxHitPoint;
            }
        }
    }

    public float moveSpeed { get; private set; }

    private void Awake()
    {
        owner = GetComponent<Player>();
    }

    private void Start()
    {
        hitPoint = _maxHitPoint;
        moveSpeed = _defaultMoveSpeed;
        isInitialized = true;
    }

    public void CorrectDeadPosition(Vector2 pos)
    {
        transform.position = pos;
    }
    public bool isDead => _hitPoint <= 0;
    public int MaxHitPoint => _maxHitPoint;
}
