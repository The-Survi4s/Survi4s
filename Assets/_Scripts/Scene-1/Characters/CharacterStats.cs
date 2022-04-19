using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [SerializeField] private int MaxHitPoint;
    [SerializeField] private float DefaultMoveSpeed;
    public event Action OnPlayerDead;
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

            if (_hitPoint > MaxHitPoint)
            {
                _hitPoint = MaxHitPoint;
            }
        }
    }

    public float moveSpeed { get; private set; }

    private void Start()
    {
        hitPoint = MaxHitPoint;
        moveSpeed = DefaultMoveSpeed;
    }

    public void CorrectDeadPosition(float xPos, float yPos)
    {
        transform.position = new Vector2(xPos, yPos);
    }
    public bool isDead => _hitPoint <= 0;
}
