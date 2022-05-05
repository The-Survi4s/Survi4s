using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

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
    public Statue statue { get; private set; }
    private readonly List<Wall> _walls = new List<Wall>();
    private readonly Dictionary<Monster.Origin, List<Wall>> _wallDictionary =
        new Dictionary<Monster.Origin, List<Wall>>();
    private int _maxWallId = 0;
    public int maxWallHp => 100;

    [SerializeField] private Tilemap _wallTilemap;
    [SerializeField] private TileStages _wallTileStages;
    [SerializeField] private TileStages _statueTileStages;
    private Dictionary<Vector3Int, Wall> _wallTiles = new Dictionary<Vector3Int, Wall>();

    public int GetNewWallId() => _maxWallId++;
    public Monster.Origin GetOriginFromWorldPos(Vector3 position)
    {
        var angle = Mathf.Atan2(position.y, position.x) * Mathf.Rad2Deg;
        angle += angle < 0 ? 360 : 0;
        //Debug.Log(position + ", " + angle + ", " + Mathf.FloorToInt(((angle + 360 + 45) % 360) / 90));
        return (Monster.Origin)Mathf.Clamp(Mathf.FloorToInt(((angle + 360 + 45) % 360) / 90), 0, 3);
    }
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

    public void SetStatue(Statue statueObj)
    {
        if (statue) Destroy(statueObj);
        statueObj.name += "Statue";
        statue = statueObj;
    }

    public Vector3Int GetCellPosition(Vector3 worldPos)
    {
        return _wallTilemap.WorldToCell(worldPos);
    }

    public void ReceiveModifyWallHp(int id, float amount) => GetWall(id).ModifyHp(amount);
    public void ReceiveModifyStatueHp(float amount) => statue.ModifyHp((int)amount);
    public Wall GetWall(int id) => _walls.FirstOrDefault(wall => wall.id == id);
    public Wall GetWall(Vector3Int cellPos) => _walls.FirstOrDefault(wall => wall.cellPos == cellPos);
    public Wall GetRandomNonDestroyedWallOn(Monster.Origin origin)
    {
        if(!_wallDictionary.ContainsKey(origin)) _wallDictionary.Add(origin, new List<Wall>());
        return GetRandomNonDestroyedWallFrom(_wallDictionary[origin], origin);
    }

    private Wall GetRandomNonDestroyedWallFrom(List<Wall> wallList, Monster.Origin origin)
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

    private List<Wall> GroupWalls(Monster.Origin origin)
    {
        return _walls.Where(wall => wall.origin == origin).ToList();
    }

    public Wall GetNearestNotDestroyedWallFrom(Vector3 position)
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

    // Event -------------------------------------------------------------------------

    private void OnDestroy()
    {
        foreach (var wall in _walls)
        {
            wall.OnDestroyed -= OnDestroyed;
            wall.OnRebuilt -= OnRebuilt;
        }
    }

    public static event Action<Wall> BroadcastWallFallen;
    public static event Action<Statue> BroadcastStatueFallen;
    public static event Action<Monster.Origin> BroadcastWallRebuilt;

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
                break;
        }
        
    }
    private static void OnRebuilt(DestroyableTile obj)
    {
        switch (obj)
        {
            case Wall wall:
                NavMeshController.UpdateNavMesh();
                BroadcastWallRebuilt?.Invoke(wall.origin);
                break;
            case Statue statue:
                //Statue ngga bisa di rebuilt
                break;
        }
    }

    public void UpdateWallTilemap(DestroyableTile tile)
    {
        var tileStages = tile switch
        {
            Wall _ => _wallTileStages,
            Statue _ => _statueTileStages,
            _ => null
        };
        if(!tileStages) return;
        var variantCount = tileStages.getTileStages.Length - 1;
        var variantId = variantCount - Mathf.FloorToInt(tile.hp / (tile.maxHp / variantCount));
        if (tile.spriteVariantId == variantId) return;
        Debug.Log($"Variant count {variantCount} - Floor({tile.hp}/{tile.maxHp}/variant count)");
        Debug.Log($"Tile on {tile.cellPos} = level [{tile.spriteVariantId}] => [{variantId}]. {tile.name} at {100*tile.hp/tile.maxHp}% HP");
        tile.spriteVariantId = variantId;
        var cellPos = tile.cellPos;
        var go = _wallTilemap.GetObjectToInstantiate(tile.cellPos);
        GameObject go2;
        if(go)
        {
            go2 = Instantiate(go);
        }
        else
        {
            Debug.Log($"tile {tile} has no gameObject!");
            return;
        }

        if (variantId == variantCount)
        {
            _wallTilemap.SetTile(tile.cellPos, null);
            //Debug.Log($"Tile at {tile.cellPos} set to null");
        }
        else
        {
            _wallTilemap.SetTile(tile.cellPos, tileStages.GetTileStage(variantId));
            go2.transform.SetParent(_wallTilemap.transform);
            //Debug.Log($"Tile at {cellPos} set to {tileStages.GetTileStage(variantId)}");
        }
        _wallTilemap.RefreshTile(cellPos);
        NavMeshController.UpdateNavMesh();
    }
}
