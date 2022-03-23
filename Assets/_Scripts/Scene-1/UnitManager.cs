using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class UnitManager : MonoBehaviour
{
    // Prefab -------------------------------------------------------------------------
    [SerializeField] private GameObject playerPrefab;

    // List ---------------------------------------------------------------------------
    public List<GameObject> players;
    [SerializeField] public List<WeaponBase> weapons;
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
    // Receive
    public void AddMonster(Monster monster)
    {
        monster.SetTargetWall(WallManager.Instance.GetRandomWallOn(monster.origin));
        monsters.Add(monster);
    }
}
