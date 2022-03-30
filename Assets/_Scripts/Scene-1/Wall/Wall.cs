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
    public bool isInitialized { get; private set; }
    public static event Action<Wall> OnWallDestroyed;
    public static event Action<Wall> OnWallRebuilt;
    public Monster.Origin origin { get; private set; } // Di set di inspector

    private void Start()
    {
        isInitialized = false;
        Init(WallManager.Instance.GetNewWallID());
        WallManager.Instance.AddWall(this); // Auto add
    }

    public void Init(int id)
    {
        ID = id;
        hitPoint = DefaultHitPoint;
        isInitialized = true;
        isDestroyed = false;
    }

    public void ModifyWallHp(float amount)
    {
        hitPoint += amount;
        if (hitPoint > 0)
        {
            if (!isDestroyed) return;
            GetComponent<Collider2D>().enabled = true;
            isDestroyed = false;
            OnWallRebuilt?.Invoke(this);
        }
        else if (hitPoint <= 0)
        {
            hitPoint = 0;
            if (isDestroyed) return;
            GetComponent<Collider2D>().enabled = false;
            isDestroyed = true;
            // Tell all that this is destroyed
            OnWallDestroyed?.Invoke(this);
        }
    }
}
