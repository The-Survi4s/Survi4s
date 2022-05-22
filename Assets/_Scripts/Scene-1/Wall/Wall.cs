using UnityEngine;
using UnityEngine.AI;

public class Wall : DestroyableTile
{
    public int id { get; private set; }
    public override int maxHp { get; protected set; }
    private bool _isInitialized = false;
    
    [field: SerializeField] public Origin origin { get; private set; } // Di set di inspector

    private void Start()
    {
        EnableWall(true);
        Init(TilemapManager.instance.GetNewWallId(), 
            TilemapManager.instance.GetOrigin(transform.position),
            TilemapManager.instance.ToCellPosition(transform.position));
        TilemapManager.instance.AddWall(this); // Auto add
    }

    /// <summary>
    /// Initialize <see cref="Wall"/>'s fields. Valid only once. 
    /// </summary>
    /// <param name="id"><see cref="Wall"/>'s id</param>
    /// <param name="wallOrigin">The <see cref="Origin"/> of this <see cref="Wall"/></param>
    /// <param name="cellPosition">The cell position of this <see cref="Wall"/>. Not a world position</param>
    /// <param name="initialHp">0 is <see cref="maxHp"/>. Set initial <see cref="Wall"/> hp here</param>
    /// <returns></returns>
    public bool Init(int id, Origin wallOrigin, Vector3Int cellPosition, int initialHp = 0)
    {
        if (_isInitialized) return false;
        this.id = id;
        origin = wallOrigin;
        maxHp = TilemapManager.instance.maxWallHp;
        hp = initialHp == 0 ? maxHp : initialHp;
        cellPos = cellPosition;
        _isInitialized = true;
        isDestroyed = false;
        return true;
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
