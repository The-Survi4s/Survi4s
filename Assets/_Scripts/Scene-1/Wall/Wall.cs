using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Wall : MonoBehaviour
{
    [field: SerializeField] public float hitPoint { get; private set; }
    public Vector3Int cellPos { get; private set; }
    [field: SerializeField] public int Id { get; private set; }
    public bool isDestroyed { get; private set; }
    public bool isInitialized { get; private set; }
    public event Action<Wall> OnWallDestroyed;
    public event Action<Wall> OnWallRebuilt;
    private NavMeshModifier _navMesh;
    private Collider2D _collider;
    [field: SerializeField] public Monster.Origin origin { get; private set; } // Di set di inspector

    private void Start()
    {
        isInitialized = false;
        _navMesh = GetComponent<NavMeshModifier>();
        _collider = GetComponent<Collider2D>();
        EnableWall(true);
        Init(WallManager.instance.GetNewWallId(), 
            WallManager.instance.GetOriginFromWorldPos(transform.position),
            WallManager.instance.GetCellPosition(transform.position));
        WallManager.instance.AddWall(this); // Auto add
    }

    public void Init(int id, Monster.Origin _origin, Vector3Int cellPosition)
    {
        Id = id;
        this.origin = _origin;
        hitPoint = WallManager.instance.maxWallHp;
        cellPos = cellPosition;
        isInitialized = true;
    }

    public void ModifyWallHp(float amount)
    {
        hitPoint += amount;
        if (hitPoint > 0)
        {
            if (hitPoint > WallManager.instance.maxWallHp) hitPoint = WallManager.instance.maxWallHp;
            if (!isDestroyed) return;
            EnableWall(true);
            Debug.Log($"Wall {Id} from {origin} has been rebuilt!");
            OnWallRebuilt?.Invoke(this);
        }
        else if (hitPoint <= 0)
        {
            hitPoint = 0;
            if (isDestroyed) return;
            EnableWall(false);
            // Tell all that this is destroyed
            Debug.Log($"Wall {Id} from {origin} has been destroyed");
            OnWallDestroyed?.Invoke(this);
        }
        WallManager.instance.UpdateWall(this);
    }

    private void EnableWall(bool wallEnabled)
    {
        _collider.enabled = wallEnabled;
        _navMesh.ignoreFromBuild = !wallEnabled;
        isDestroyed = !wallEnabled;
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
