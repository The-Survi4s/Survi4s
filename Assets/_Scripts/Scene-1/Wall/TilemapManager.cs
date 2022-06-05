using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

/// <summary>
/// The direction of which something comes from
/// </summary>
public enum Origin { Right, Top, Left, Bottom }

public class TilemapManager : MonoBehaviour
{
    public static TilemapManager instance { get; private set; }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    #region Statue Handlers

    /// <summary>
    /// Singleton of the <see cref="Statue"/>
    /// </summary>
    public Statue statue { get; private set; }

    /// <summary>
    /// Set in inspector. <see cref="TileStages"/> of <see cref="Statue"/>
    /// </summary>
    [SerializeField] private TileStages _statueTileStages;

    /// <summary>
    /// Sets the main <see cref="Statue"/>. Will fail if <see cref="statue"/> is not <see langword="null"/>
    /// </summary>
    /// <param name="statueObj"></param>
    public void SetStatue(Statue statueObj)
    {
        if (statue)
        {
            Destroy(statueObj);
            return;
        }
        statueObj.name = "Statue";
        statue = statueObj;
    }

    // Used by Network Client
    public void ModifyStatueHp(float amount) => statue.ModifyHp((int)amount);

    #endregion

    #region Wall Handlers

    #region Wall Variables

    /// <summary>
    /// The main list of <see cref="Wall"/>s
    /// </summary>
    private readonly List<Wall> _walls = new List<Wall>();

    /// <summary>
    /// The main list of <see cref="Wall"/>s, but separated by <see cref="Origin"/>
    /// </summary>
    private readonly Dictionary<Origin, List<Wall>> _wallDictionary =
        new Dictionary<Origin, List<Wall>>();

    /// <summary>
    /// This keeps track of how many walls have been spawned. Used as a unique id for each new wall. 
    /// </summary>
    private int _maxWallId = 0;

    /// <summary>
    /// Set in inspector. The maximum and initial <see cref="Wall"/> hp. 
    /// </summary>
    public int maxWallHp => 100;

    /// <summary>
    /// Set in inspector. The <see cref="Tilemap"/> of all <see cref="Wall"/>s
    /// </summary>
    [SerializeField] private Tilemap _wallTilemap;

    /// <summary>
    /// Set in inspector. <see cref="TileStages"/> of <see cref="Wall"/>s
    /// </summary>
    [SerializeField] private TileStages _wallTileStages;

    /// <summary>
    /// <see cref="Wall"/>s mapped by their <see cref="Vector3Int"/> cell position
    /// </summary>
    private readonly Dictionary<Vector3Int, Wall> _wallTiles = new Dictionary<Vector3Int, Wall>();

    /// <summary>
    /// The <see cref="BrokenWall"/> <see cref="GameObject"/> to spawn
    /// </summary>
    [SerializeField] private GameObject _brokenWallPrefab;

    /// <summary>
    /// <see cref="BrokenWall"/>s mapped by their <see cref="Vector3Int"/> cell position
    /// </summary>
    private readonly Dictionary<Vector3Int, BrokenWall> _brokenWalls = new Dictionary<Vector3Int, BrokenWall>();

    #endregion

    #region Wall Methods

    // Methods to add and remove walls from list
    public void AddWall(Wall wall)
    {
        wall.OnDestroyed += OnDestroyed;
        wall.OnRebuilt += OnRebuilt;
        wall.gameObject.name = $"Wall {wall.id} {wall.origin}";
        _walls.Add(wall);
        if (!_wallTiles.ContainsKey(wall.cellPos)) _wallTiles.Add(wall.cellPos, wall);
        else _wallTiles[wall.cellPos] = wall;
    }

    public void RemoveWall(Wall wall)
    {
        _wallTiles.Remove(wall.cellPos);
        _walls.Remove(wall);
        _wallDictionary[wall.origin] = GroupWalls(wall.origin);
        _wallTiles.Remove(wall.cellPos);
    }

    public int GetNewWallId() => _maxWallId++;

    /// <summary>
    /// Spawns a <see cref="BrokenWall"/> in <paramref name="wall"/>'s position
    /// </summary>
    /// <param name="wall">The <see cref="Wall"/> to copy values from</param>
    private void SpawnBrokenWall(Wall wall)
    {
        var brokenWallGameObject = Instantiate(_brokenWallPrefab, wall.transform.position, Quaternion.identity,
            _wallTilemap.transform);
        brokenWallGameObject.name = "Broken Wall " + wall.id;
        var brokenWall = brokenWallGameObject.GetComponent<BrokenWall>();
        brokenWall.Init(wall.id, wall.cellPos, wall.origin);
        _brokenWalls.Add(brokenWall.cellPos, brokenWall);
        brokenWall.OnRebuilt += OnRebuilt;
    }

    /// <summary>
    /// Destroys a <see cref="BrokenWall"/> on <paramref name="cellPos"/> 
    /// then sets a <see cref="Wall"/> tile on it with 10 Hp.
    /// </summary>
    /// <param name="cellPos"></param>
    public void RemoveBrokenWall(Vector3Int cellPos)
    {
        if (_brokenWalls.ContainsKey(cellPos))
        {
            Debug.Log("BW found");
            var bw = _brokenWalls[cellPos];
            _brokenWalls.Remove(cellPos);
            bw.OnRebuilt -= OnRebuilt;
            Destroy(bw);

            _wallTilemap.SetTile(cellPos, _wallTileStages.GetTile(_wallTileStages.getTileStages.Count - 1));
            var wall = _wallTilemap.GetInstantiatedObject(cellPos).GetComponent<Wall>();
            wall.Init(GetNewWallId(), GetOrigin(wall.transform.position), cellPos, 10);
            wall.ModifyHp(0);
            _wallTilemap.RefreshTile(cellPos);
            NavMeshController.UpdateNavMesh();
        }
    }

    // Methods used by Network Client class
    public void ModifyWallHp(int id, float amount) => GetWall(id).ModifyHp(amount);

    public void RebuiltWall(int id, float amount) => GetWall(id, true).ModifyHp(amount);

    /// <summary>
    /// Gets a <see cref="DestroyableTile"/> with a matching <paramref name="id"/>
    /// </summary>
    /// <param name="id"> The <c>id</c> of the <see cref="DestroyableTile"/> </param>
    /// <param name="isBroken"> set to true to search for <see cref="BrokenWall"/> instead of <see cref="Wall"/></param>
    /// <returns><see cref="Wall"/> if not <paramref name="isBroken"/>. Or <see cref="BrokenWall"/> if <paramref name="isBroken"/></returns>
    public DestroyableTile GetWall(int id, bool isBroken = false) => !isBroken
        ? _walls.FirstOrDefault(wall => wall.id == id) as DestroyableTile
        : _brokenWalls.Values.FirstOrDefault(bw => bw.id == id);

    /// <summary>
    /// Gets a <see cref="Wall"/> with a matching <paramref name="cellPos"/>
    /// </summary>
    /// <param name="cellPos"> Cell position of the wall </param>
    /// <returns></returns>
    public Wall GetWall(Vector3Int cellPos) => _walls.FirstOrDefault(wall => wall.cellPos == cellPos);

    /// <summary>
    /// Gets a random <see cref="Wall"/> with a matching <paramref name="origin"/>
    /// </summary>
    /// <param name="origin"> Where is it originates from </param>
    /// <returns></returns>
    public Wall GetWall(Origin origin)
    {
        if (!_wallDictionary.ContainsKey(origin)) _wallDictionary.Add(origin, new List<Wall>());
        return GetWall(_wallDictionary[origin], origin);
    }

    /// <summary>
    /// Gets a <see cref="Random"/> <see cref="Wall"/> from <paramref name="wallList"/>.
    /// </summary>
    /// <remarks>
    /// Fills the <paramref name="wallList"/> with <see cref="Wall"/>s with matching <paramref name="origin"/> if it's empty. 
    /// </remarks>
    /// <param name="wallList"> the list of <see cref="Wall"/>s to pick a wall from</param>
    /// <param name="origin"> The origin of the <see cref="Wall"/></param>
    /// <returns>A random <see cref="Wall"/>. Returns <see langword="null"/> if <paramref name="wallList"/> is <see cref="_walls"/></returns>
    private Wall GetWall(List<Wall> wallList, Origin origin)
    {
        if (wallList == _walls)
        {
            Debug.Log("Warning, list walls mau dirubah! Returning null");
            return null;
        }
        if (wallList.Count == 0)
        {
            wallList = GroupWalls(origin);
        }
        var nonDestroyedWallList = wallList.Where(wall => !wall.isDestroyed).ToList();
        return nonDestroyedWallList[Random.Range(0, wallList.Count)];
    }

    /// <summary>
    /// Gets the nearest <see cref="Wall"/> from a <paramref name="position"/>
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Wall GetWall(Vector3 position)
    {
        var dist = float.MaxValue;
        var res = _walls[0];
        foreach (var wall in _walls.Where(w => !w.isDestroyed).ToList())
        {
            var dist2 = Vector3.Distance(position, wall.transform.position);
            if (dist2 < dist)
            {
                dist = dist2;
                res = wall;
            }
        }

        return res;
    }

    /// <summary>
    /// Gets a <see cref="List{Wall}"/> of matching <paramref name="origin"/> from <see cref="_walls"/>
    /// </summary>
    /// <param name="origin"> The origin of the <see cref="Wall"/></param>
    /// <returns></returns>
    private List<Wall> GroupWalls(Origin origin) => _walls.Where(wall => wall.origin == origin).ToList();

    #endregion

    #endregion

    #region Utilities

    /// <summary>
    /// Converts world <paramref name="position"/> to <see cref="Origin"/>
    /// </summary>
    /// <remarks>With <see cref="Vector3.zero"/> as it's center,
    /// it decides if a position is in an area on top, left, below, or on right of the center.
    /// Then returns the corresponding <see cref="Origin"/>. </remarks>
    /// <param name="position"> world position </param>
    /// <returns></returns>
    public Origin GetOrigin(Vector3 position)
    {
        var angle = Mathf.Atan2(position.y, position.x) * Mathf.Rad2Deg;
        angle += angle < 0 ? 360 : 0; 
        //Debug.Log(position + ", " + angle + ", " + Mathf.FloorToInt(((angle + 360 + 45) % 360) / 90));
        return (Origin)Mathf.Clamp(Mathf.FloorToInt(((angle + 360 + 45) % 360) / 90), 0, 3);
    }

    /// <summary>
    /// Converts <paramref name="worldPos"/> to cell position
    /// </summary>
    /// <param name="worldPos">The position in world space</param>
    /// <returns></returns>
    public Vector3Int WorldToCell(Vector3 worldPos) => _wallTilemap.WorldToCell(worldPos);

    public Vector3 CellToWorld(Vector3Int cellPos) => _wallTilemap.CellToWorld(cellPos);

    #endregion

    #region Events
    // Event -------------------------------------------------------------------------
    private void OnDestroy()
    {
        foreach (var wall in _walls)
        {
            wall.OnDestroyed -= OnDestroyed;
        }

        foreach (var brokenWall in _brokenWalls)
        {
            brokenWall.Value.OnRebuilt -= OnRebuilt;
        }
    }

    public static event Action<Wall> BroadcastWallFallen;
    public static event Action<Statue> BroadcastStatueFallen;
    public static event Action<Origin> BroadcastWallRebuilt;

    private static void OnDestroyed(DestroyableTile obj)
    {
        switch (obj)
        {
            case Wall wall:
                NavMeshController.UpdateNavMesh();
                BroadcastWallFallen?.Invoke(wall);
                break;
            case Statue s:
                BroadcastStatueFallen?.Invoke(s);
                GameManager.Instance.ChangeState(GameManager.GameState.GameOver);
                break;
        }
    }

    private static void OnRebuilt(DestroyableTile obj)
    {
        if (!(obj is BrokenWall brokenWall)) return;
        NavMeshController.UpdateNavMesh();
        BroadcastWallRebuilt?.Invoke(brokenWall.origin);
        brokenWall.RemoveFromMap();
    }

    #endregion

    /// <summary>
    /// Updates a specific <paramref name="tile"/> on <see cref="_wallTilemap"/> to be it's variant (<see cref="TileStages"/>).
    /// <br/>Then updates the <see cref="NavMesh"/>
    /// </summary>
    /// <remarks>
    /// Spawns a <see cref="BrokenWall"/> when a <see cref="Wall"/>'s hp is 0. <br/><br/>
    /// Must NOT <see langword="null"/>: <br/><code>    <see cref="_wallTileStages"/></code><br/><code>    <see cref="_statueTileStages"/></code>
    /// </remarks>
    /// <param name="tile"> The tile to be updated </param>
    public void UpdateWallTilemap(DestroyableTile tile)
    {
        // Choosing the right Tile Stages
        var tileStages = tile switch
        {
            Wall _ => _wallTileStages,
            Statue _ => _statueTileStages,
            _ => null
        };
        if(!tileStages) return;

        // Calculate variant id
        var variantCount = tileStages.getTileStages.Count - 1;
        var variantId = variantCount - Mathf.FloorToInt(tile.hp / (tile.maxHp * 1.0f / variantCount));
        if (tile.spriteVariantId == variantId) return;
        
        tile.spriteVariantId = variantId;
        var cellPos = tile.cellPos;

        // If wall is destroyed variant, spawn brokenWall
        if (tile.hp <= 0 && tile is Wall wall) SpawnBrokenWall(wall);

        // Store does this location has a navmesh
        var hasNavMesh = NavMesh.SamplePosition(CellToWorld(cellPos), out _, 0.1f, NavMesh.AllAreas);

        // Set tile, then update Tilemap and NavMesh
        TileBase newTile = variantId == variantCount ? null : tileStages.GetTile(variantId);
        _wallTilemap.SetTile(cellPos, newTile);
        _wallTilemap.RefreshTile(cellPos);

        Debug.Log($"tile at {cellPos} hasNavMesh:{hasNavMesh}, newTile:{newTile}.");
        if(hasNavMesh && !newTile) NavMeshController.UpdateNavMesh();
        else if(!hasNavMesh && newTile) NavMeshController.UpdateNavMesh();
    }

    /// <summary>
    /// Gets a new position <paramref name="jumpDistance"/> away in <paramref name="direction"/> 
    /// from <paramref name="worldPosFrom"/>. 
    /// </summary>
    /// <param name="worldPosFrom"></param>
    /// <param name="direction"></param>
    /// <param name="jumpDistance"></param>
    /// <returns>
    /// The new position. <br/>
    /// If a collider is found at the new position, 
    /// returns <paramref name="worldPosFrom"/>
    /// </returns>
    public Vector3 GetJumpPos(Vector3 worldPosFrom, Vector3 direction, int jumpDistance = 2)
    {
        var dir = WorldToCell(direction);
        dir.Clamp(-Vector3Int.one, Vector3Int.one);
        var from = WorldToCell(worldPosFrom);
        var wallInFront = GetWall(from + dir);
        if (wallInFront && IsJumpPossible(from, dir, jumpDistance))
        {
            return CellToWorld(from + dir * jumpDistance);
        }
        else return worldPosFrom;
    }

    /// <summary>
    /// Checks if there's no collider in 2 locations: <br/>
    /// <paramref name="cellPos"/>, 
    /// and a location <paramref name="jumpDistance"/> away
    /// in <paramref name="direction"/> from <paramref name="cellPos"/>
    /// </summary>
    /// <param name="cellPos"></param>
    /// <param name="direction"></param>
    /// <param name="jumpDistance"></param>
    /// <returns></returns>
    private bool IsJumpPossible(Vector3Int cellPos, Vector3Int direction, int jumpDistance)
    {
        int wallLayer = (int)Mathf.Log(LayerMask.GetMask("Wall"), 2);

        Vector2Int pos = new Vector2Int(cellPos.x, cellPos.y);
        Vector2Int dir = new Vector2Int(direction.x, direction.y);
        dir.Clamp(-Vector2Int.one, Vector2Int.one);
        var tileCollider1 = Physics2D.OverlapPoint(pos, wallLayer);
        var tileCollider2 = Physics2D.OverlapPoint(pos + dir * jumpDistance, wallLayer);
        //If collider found, jump not possible
        if (tileCollider1 || tileCollider2) return false;
        return true;
    }
}
