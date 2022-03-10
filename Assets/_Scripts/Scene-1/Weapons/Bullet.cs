using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float DefaultMoveSpeed;
    public float moveSpeed { get; private set; }

    private bool rotationIsSet;

    private void Start()
    {
        moveSpeed = DefaultMoveSpeed;
    }

    private void Update()
    {
        if (rotationIsSet)
        {
            // Move bullet

        }
    }

    public void SetRotation(Vector2 mousePos)
    {
        
    }
}
