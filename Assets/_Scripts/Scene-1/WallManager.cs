using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class WallManager : MonoBehaviour
{
    private static WallManager _instance;
    public static WallManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new WallManager();
            }
            return _instance;
        }
    }

    [SerializeField] private List<Wall> walls;
    private List<Wall> _topWalls;
    private List<Wall> _bottomWalls;
    private List<Wall> _rightWalls;
    private List<Wall> _leftWalls;
    private int _maxWallID;

    public int GetNewWallID() => _maxWallID++;
    public void AddWall(Wall wall) => walls.Add(wall);

    public void ReceiveModifyWallHp(int id, float amount) => GetWall(id).ModifyWallHp(amount);
    private Wall GetWall(int id) => walls.FirstOrDefault(wall => wall.ID == id);
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
        return wallList[Random.Range(0, wallList.Count - 1)];
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

    public static event Action<Wall> OnWallFallenTop;
    public static event Action<Wall> OnWallFallenRight;
    public static event Action<Wall> OnWallFallenBottom;
    public static event Action<Wall> OnWallFallenLeft;
    private static void OnWallDestroyed(Wall wall)
    {
        switch (wall.origin)
        {
            case Monster.Origin.Top:
                OnWallFallenTop?.Invoke(wall);
                break;
            case Monster.Origin.Right:
                OnWallFallenRight?.Invoke(wall);
                break;
            case Monster.Origin.Bottom:
                OnWallFallenBottom?.Invoke(wall);
                break;
            case Monster.Origin.Left:
                OnWallFallenLeft?.Invoke(wall);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    private void OnWallRebuilt(Wall wall)
    {
        throw new NotImplementedException();
    }
}
