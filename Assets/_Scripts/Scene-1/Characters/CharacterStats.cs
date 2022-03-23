using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [SerializeField] private int MaxHitPoint;
    [SerializeField] private float DefaultMoveSpeed;

    private float _hitPoint;
    public float hitPointAdd
    {
        get => _hitPoint;
        set
        {
            _hitPoint += value;

            if (_hitPoint <= 0)
            {
                // Dead
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
        hitPointAdd = MaxHitPoint;
        moveSpeed = DefaultMoveSpeed;
    }

    public void PlayerDead(float xPos, float yPos)
    {
        transform.position = new Vector2(xPos, yPos);
    }
    public bool isDead => _hitPoint <= 0;
}
