using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class WallManager : MonoBehaviour
{
    public static WallManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private List<Wall> walls = new List<Wall>();
    private List<Wall> _topWalls = new List<Wall>();
    private List<Wall> _bottomWalls = new List<Wall>();
    private List<Wall> _rightWalls = new List<Wall>();
    private List<Wall> _leftWalls = new List<Wall>();
    private int _maxWallId = 0;

    public int GetNewWallId() => _maxWallId++;
    public void AddWall(Wall wall) => walls.Add(wall);

    public void ReceiveModifyWallHp(int id, float amount) => GetWall(id).ModifyWallHp(amount);
    public void ReceiveModifyStatueHp(float amount) => GameManager.Instance.statue.ModifyHp((int)amount);
    private Wall GetWall(int id) => walls.FirstOrDefault(wall => wall.Id == id);
    public Wall GetRandomWallOn(Monster.Origin origin)
    {
        return origin switch
        {
            Monster.Origin.Top => GetRandomWallFrom(_topWalls, origin),
            Monster.Origin.Right => GetRandomWallFrom(_rightWalls, origin),
            Monster.Origin.Bottom => GetRandomWallFrom(_bottomWalls, origin),
            Monster.Origin.Left => GetRandomWallFrom(_leftWalls, origin),
            _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, null)
        };
    }

    private Wall GetRandomWallFrom(List<Wall> wallList, Monster.Origin origin)
    {
        if (wallList == walls) return null;
        if (wallList.Count == 0)
        {
            wallList = GroupWalls(origin);
        }
        return wallList[Random.Range(0, wallList.Count)];
    }

    private List<Wall> GroupWalls(Monster.Origin origin)
    {
        return walls.Where(wall => wall.origin == origin).ToList();
    }

    // Event -------------------------------------------------------------------------
    private void OnEnable()
    {
        Wall.OnWallDestroyed += OnWallDestroyed;
        Wall.OnWallRebuilt += OnWallRebuilt;
    }

    private void OnDisable()
    {
        Wall.OnWallDestroyed -= OnWallDestroyed;
        Wall.OnWallRebuilt -= OnWallRebuilt;
    }

    public static event Action<Wall> BroadcastWallFallen;
    public static event Action<Monster.Origin> BroadcastWallRebuilt;

    private static void OnWallDestroyed(Wall wall)
    {
        wall.GetComponent<NavMeshModifier>().overrideArea = false;
        NavMeshController.UpdateNavMesh();
        BroadcastWallFallen?.Invoke(wall);
    }
    private void OnWallRebuilt(Wall wall)
    {
        wall.GetComponent<NavMeshModifier>().overrideArea = true;
        NavMeshController.UpdateNavMesh();
        BroadcastWallRebuilt?.Invoke(wall.origin);
    }
}
