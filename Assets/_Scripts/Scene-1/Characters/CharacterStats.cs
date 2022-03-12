using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [SerializeField] private int DefaultHitPoint;
    [SerializeField] private float DefaultMoveSpeed;

    public float hitPoint { get; private set; }
    public float moveSpeed { get; private set; }

    private void Start()
    {
        hitPoint = DefaultHitPoint;
        moveSpeed = DefaultMoveSpeed;
    }

    public void ReduceHitPoint(float damage)
    {
        hitPoint -= damage;

        if (hitPoint <= 0)
        {
            // Dead

        }
    }
    public void HealHitPoint(float healPoint)
    {
        hitPoint += hitPoint;
    }
    public void PlayerDead(float xPos, float yPos)
    {
        transform.position = new Vector2(xPos, yPos);
    }
}
