using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    [SerializeField] private float DefaultHitPoint;
    [field: SerializeField] public float hitPoint { get; private set; }

    [field: SerializeField] public int Id { get; private set; }
    public bool isDestroyed { get; private set; }
    public bool isInitialized { get; private set; }
    public event Action<Wall> OnWallDestroyed;
    public event Action<Wall> OnWallRebuilt;
    [field: SerializeField] public Monster.Origin origin { get; private set; } // Di set di inspector

    private void Start()
    {
        isInitialized = false;
        Init(WallManager.Instance.GetNewWallId());
        WallManager.Instance.AddWall(this); // Auto add
    }

    public void Init(int id)
    {
        Id = id;
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
            Debug.Log($"Wall {Id} from {origin} has been rebuilt!");
            OnWallRebuilt?.Invoke(this);
        }
        else if (hitPoint <= 0)
        {
            hitPoint = 0;
            if (isDestroyed) return;
            GetComponent<Collider2D>().enabled = false;
            isDestroyed = true;
            // Tell all that this is destroyed
            Debug.Log($"Wall {Id} from {origin} has been destroyed");
            OnWallDestroyed?.Invoke(this);
        }
    }

    // ----------- cheats
    [ContextMenu(nameof(DamageWallBy10))]
    private void DamageWallBy10()
    {
        ModifyWallHp(-10);
    }
    [ContextMenu(nameof(HealWallBy10))]
    private void HealWallBy10()
    {
        ModifyWallHp(10);
    }
}
