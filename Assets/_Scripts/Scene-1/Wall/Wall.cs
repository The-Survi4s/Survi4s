using UnityEngine;
using UnityEngine.AI;

public class Wall : DestroyableTile
{
    [field: SerializeField] public int id { get; private set; }
    public override int maxHp { get; protected set; }
    private bool _isInitialized = false;

    private NavMeshModifier _navMesh;
    private Collider2D _collider;
    [field: SerializeField] public Monster.Origin origin { get; private set; } // Di set di inspector

    private void Start()
    {
        _isInitialized = false;
        _navMesh = GetComponent<NavMeshModifier>();
        _collider = GetComponent<Collider2D>();
        EnableWall(true);
        Init(TilemapManager.instance.GetNewWallId(), 
            TilemapManager.instance.GetOriginFromWorldPos(transform.position),
            TilemapManager.instance.GetCellPosition(transform.position));
        TilemapManager.instance.AddWall(this); // Auto add
    }

    public void Init(int id, Monster.Origin wallOrigin, Vector3Int cellPosition)
    {
        if (_isInitialized) return;
        this.id = id;
        origin = wallOrigin;
        maxHp = TilemapManager.instance.maxWallHp;
        hp = maxHp;
        cellPos = cellPosition;
        _isInitialized = true;
        isDestroyed = false;
    }

    protected override void AfterModifyHp()
    {
        TilemapManager.instance.UpdateWallTilemap(this);
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
        _collider.enabled = wallEnabled;
        _navMesh.ignoreFromBuild = !wallEnabled;
        isDestroyed = !wallEnabled;
    }

    // ----------- cheats
    [ContextMenu(nameof(DamageWallBy10))]
    private void DamageWallBy10()
    {
        ModifyHp(-10);
    }
    [ContextMenu(nameof(HealWallBy10))]
    private void HealWallBy10()
    {
        ModifyHp(10);
    }
}
