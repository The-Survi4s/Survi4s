using UnityEngine;
using UnityEngine.AI;

public class Wall : DestroyableTile
{
    public int id { get; private set; }
    public override int maxHp { get; protected set; }
    private bool _isInitialized = false;
    
    [field: SerializeField] public Monster.Origin origin { get; private set; } // Di set di inspector

    private void Start()
    {
        _isInitialized = false;
        EnableWall(true);
        Init(TilemapManager.instance.GetNewWallId(), 
            TilemapManager.instance.GetOriginFromWorldPos(transform.position),
            TilemapManager.instance.GetCellPosition(transform.position));
        TilemapManager.instance.AddWall(this); // Auto add
    }

    public void Init(int id, Monster.Origin wallOrigin, Vector3Int cellPosition, int initialHp = 0)
    {
        if (_isInitialized) return;
        this.id = id;
        origin = wallOrigin;
        maxHp = TilemapManager.instance.maxWallHp;
        hp = initialHp == 0 ? maxHp : initialHp;
        cellPos = cellPosition;
        _isInitialized = true;
        isDestroyed = false;
    }

    protected override void InvokeRebuiltEvent()
    {
        if (!isDestroyed) return;
        base.InvokeRebuiltEvent();
        EnableWall(true);
        Debug.Log($"Wall {id} from {origin} has been rebuilt!");
    }

    protected override void InvokeDestroyedEvent()
    {
        if (isDestroyed) return;
        base.InvokeDestroyedEvent();
        EnableWall(false);
        // Tell all that this is destroyed
        Debug.Log($"Wall {id} from {origin} has been destroyed");
    }

    private void EnableWall(bool wallEnabled)
    {
        isDestroyed = !wallEnabled;
    }

    private void OnDestroy()
    {
        TilemapManager.instance.RemoveWall(this);
    }
}
