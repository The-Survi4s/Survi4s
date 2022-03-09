using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [SerializeField] private int DefaultHitPoint;
    [SerializeField] private float DefaultMoveSpeed;

    public int hitPoint { get; private set; }
    public float moveSpeed { get; private set; }

    private void Start()
    {
        hitPoint = DefaultHitPoint;
        moveSpeed = DefaultMoveSpeed;
    }
}
