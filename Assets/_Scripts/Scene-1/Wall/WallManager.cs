using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class WallManager : MonoBehaviour
{
    public static WallManager instance { get; private set; }
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

    private readonly List<Wall> _walls = new List<Wall>();
    private readonly Dictionary<Monster.Origin, List<Wall>> _wallDictionary =
        new Dictionary<Monster.Origin, List<Wall>>();
    private int _maxWallId = 0;
    public int maxWallHp => 100;

    [SerializeField] private Tilemap _wallTilemap;
    [SerializeField] private TileStages _wallTileStages;
    private Dictionary<Vector3Int, Wall> _wallTiles = new Dictionary<Vector3Int, Wall>();

    public int GetNewWallId() => _maxWallId++;
    public Monster.Origin GetOriginFromWorldPos(Vector3 position)
    {
        var angle = Mathf.Atan2(position.y, position.x) * Mathf.Rad2Deg;
        angle += angle < 0 ? 360 : 0;
        Debug.Log(position + ", " + angle + ", " + Mathf.FloorToInt(((angle + 360 + 45) % 360) / 90));
        return (Monster.Origin)Mathf.FloorToInt(((angle + 360 + 45) % 360) / 90);
    }
    public void AddWall(Wall wall)
    {
        wall.OnWallDestroyed += OnWallDestroyed;
        wall.OnWallRebuilt += OnWallRebuilt;
        wall.gameObject.name = $"Wall {wall.Id} {wall.origin}";
        _walls.Add(wall);
        _wallTiles.Add(_wallTilemap.WorldToCell(wall.transform.position), wall);
    }

    public Vector3Int GetCellPosition(Vector3 worldPos)
    {
        return _wallTilemap.WorldToCell(worldPos);
    }

    public void ReceiveModifyWallHp(int id, float amount) => GetWall(id).ModifyWallHp(amount);
    public void ReceiveModifyStatueHp(float amount) => GameManager.Instance.statue.ModifyHp((int)amount);
    private Wall GetWall(int id) => _walls.FirstOrDefault(wall => wall.Id == id);
    public Wall GetRandomWallOn(Monster.Origin origin)
    {
        if(!_wallDictionary.ContainsKey(origin)) _wallDictionary.Add(origin, new List<Wall>());
        return GetRandomWallFrom(_wallDictionary[origin], origin);
    }

    private Wall GetRandomWallFrom(List<Wall> wallList, Monster.Origin origin)
    {
        if (wallList == _walls)
        {
            Debug.Log("Warning, list walls mau dirubah! Returning null");
            return null;
        }
        if (wallList.Count == 0)
        {
            wallList = _walls.Where(wall => wall.origin == origin).ToList();
        }
        return wallList[Random.Range(0, wallList.Count)];
    }

    // Event -------------------------------------------------------------------------

    private void OnDestroy()
    {
        foreach (var wall in _walls)
        {
            wall.OnWallDestroyed -= OnWallDestroyed;
            wall.OnWallRebuilt -= OnWallRebuilt;
        }
    }

    public static event Action<Wall> BroadcastWallFallen;
    public static event Action<Monster.Origin> BroadcastWallRebuilt;

    private void OnWallDestroyed(Wall wall)
    {
        NavMeshController.UpdateNavMesh();
        BroadcastWallFallen?.Invoke(wall);
    }
    private void OnWallRebuilt(Wall wall)
    {
        NavMeshController.UpdateNavMesh();
        BroadcastWallRebuilt?.Invoke(wall.origin);
    }

    public void UpdateWall(Wall wall)
    {
        var tile = _wallTilemap.GetTile(wall.cellPos);
        var stage = _wallTileStages.getTileStages.Length - Mathf.FloorToInt(wall.hitPoint / 25);
        _wallTilemap.SetTile(wall.cellPos,_wallTileStages.GetTileStage(stage));
    }
}
