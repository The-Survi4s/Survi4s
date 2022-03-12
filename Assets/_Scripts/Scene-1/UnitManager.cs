using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    // Prefab -------------------------------------------------------------------------
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject mosterPrefab;

    // List ---------------------------------------------------------------------------
    public List<GameObject> players;
    [SerializeField] public List<WeaponBase> weapons;
    public List<Wall> walls;
    public List<Monster> monsters;

    // Eazy Access --------------------------------------------------------------------
    public static UnitManager Instance { get; private set; }
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        players = new List<GameObject>();
    }

    // Spawn Player ------------------------------------------------------------------
    public void SpawnPlayer(string name, float x, float y, int skin)
    {
        Vector3 pos = new Vector3(x, y, 0);
        GameObject temp = Instantiate(playerPrefab, pos, Quaternion.identity);
        temp.name = name;
        players.Add(temp);
    }

    // Spawn Monster -----------------------------------------------------------------
    
    public void OnSpawnMonster(int ID, Monster.Origin origin)
    {
        GameObject temp = Instantiate(mosterPrefab, new Vector2(2, 0), Quaternion.identity);
        Monster monster = temp.GetComponent<Monster>();
        monster.SetID(ID);
        monster.SetOrigin(origin);
        monsters.Add(monster);
    }

    // Event -------------------------------------------------------------------------
    private void OnEnable()
    {
        Wall.OnWallDestroyed += OnWallDestroyed;
    }
    private void OnDisable()
    {
        Wall.OnWallDestroyed -= OnWallDestroyed;
    }

    public static event Action<Wall> OnWallFallenTop;
    public static event Action<Wall> OnWallFallenRight;
    public static event Action<Wall> OnWallFallenBottom;
    public static event Action<Wall> OnWallFallenLeft;
    private void OnWallDestroyed(Wall wall)
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
        }
    }
}
