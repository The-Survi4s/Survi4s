using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    [SerializeField] private float DefaultHitPoint;

    public float hitPoint { get; private set; }

    public int ID { get; private set; }
    public bool isDestroyed { get; private set; }

    public Monster.Origin origin { get; private set; }

    public static event Action<Wall> OnWallDestroyed;

    private void Start()
    {
        hitPoint = DefaultHitPoint;
    }

    public void SetOrigin(Monster.Origin ori)
    {
        origin = ori;
    }
    public void SetID(int Id)
    {
        ID = Id;
    }

    public void DamageWall(float damage)
    {
        hitPoint -= damage;

        if(hitPoint <= 0)
        {
            hitPoint = 0;
            GetComponent<Collider2D>().enabled = false;
            isDestroyed = true;

            // Tell all that this is destroyed
            OnWallDestroyed?.Invoke(this);
        }
    }
    public void RepairWall(float point)
    {
        if (isDestroyed)
        {
            GetComponent<Collider2D>().enabled = true;
            isDestroyed = false;
        }
        
        hitPoint += point;
    }
}
