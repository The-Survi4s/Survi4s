using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class UnitManager : MonoBehaviour
{
    // Prefab -------------------------------------------------------------------------

    // List ---------------------------------------------------------------------------
    private KdTree<PlayerController> _players;
    private List<GameObject> _playerUsernames;
    [SerializeField] public List<WeaponBase> weapons;
    private KdTree<Monster> _monsters;

    // Eazy Access --------------------------------------------------------------------
    public static UnitManager Instance { get; private set; }

    public int playerAliveCount => _players.Count(player => !player.isDead);
    public int playerCount => _players.Count;
    public int monsterAliveCount => _monsters.Count;

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
        _players = new KdTree<PlayerController>(true);
        _monsters = new KdTree<Monster>();
        _playerUsernames = new List<GameObject>();
    }

    private void Update()
    {
        _players.UpdatePositions();
        _monsters.UpdatePositions();

        //Update player username to whatever the hell
    }

    // Spawn Player ------------------------------------------------------------------
    public void AddPlayer(PlayerController p)
    {
        p.OnPlayerDead += HandlePlayerDead;
        _players.Add(p);

        //Get player username text object from child
    }

    // Deletion
    private void HandlePlayerDead(string idAndName)
    {
        //Set that player to
        //Set camera spectator ke player lain
    }

    public void HandlePlayerDisconnect(string idAndName)
    {
        HandlePlayerDead(idAndName);
        var index = SearchPlayerIndexById(idAndName);
        if (index >= 0) _players.RemoveAt(index);
    }

    public void DeleteMonsterFromList(int id)
    {
        var index = SearchMonsterIndexById(id);
        if(index >= 0) _monsters.RemoveAt(index);
    }

    // Spawn Monster -----------------------------------------------------------------
    // Receive
    public void AddMonster(Monster monster)
    {
        monster.SetTargetWall(WallManager.Instance.GetRandomWallOn(monster.origin));
        _monsters.Add(monster);
    }

    // Send command to units
    // Monster
    public void ModifyMonsterHp(int id, float amount)
    {
        foreach (var monster in _monsters.Where(monster => monster.id == id))
        {
            monster.ModifyHitPoint(amount);
            break;
        }
    }

    // Player
    public void SyncMousePos(string playerName, float x, float y)
    {
        var player = SearchPlayerByName(playerName);
        if (player) player.SyncMousePos(x, y);
    }

    public void SetButton(string playerName, PlayerController.Button button, bool isDown)
    {
        var player = SearchPlayerByName(playerName);
        if (player) player.SetButton(button, isDown);
    }

    public void OnEquipWeapon(string playerName, string weaponName)
    {
        var player = SearchPlayerByName(playerName);
        if (player) player.GetComponent<PlayerWeaponManager>().OnEquipWeapon(weaponName);
    }

    public void PlayAttackAnimation(string playerName)
    {
        var player = SearchPlayerByName(playerName);
        if (player) player.GetComponent<PlayerWeaponManager>().PlayAttackAnimation();
    }

    public void SpawnBullet(string playerName, float xSpawnPos, float ySpawnPos, float xMousePos, float yMousePos)
    {
        var player = SearchPlayerByName(playerName);
        if (player) player.GetComponent<PlayerWeaponManager>()
            .SpawnBullet(xSpawnPos, ySpawnPos, xMousePos, yMousePos);
    }

    public void ModifyPlayerHp(string playerName, float amount)
    {
        Debug.Log(playerName+" "+amount);
        var player = SearchPlayerByName(playerName);
        if (player) player.GetComponent<CharacterStats>().hitPoint += amount;
    }

    public void CorrectDeadPosition(string playerName, float x, float y)
    {
        var player = SearchPlayerByName(playerName);
        if (player) player.GetComponent<CharacterStats>().CorrectDeadPosition(x, y);
    }

    public void ApplyStatusEffectToMonster(int targetId, StatusEffect statusEffect, float duration, int strength)
    {
        var monster = SearchMonsterById(targetId);
        if(monster) monster.AddStatusEffect(StatusEffectFactory.CreateNew(monster, statusEffect, duration, strength));
    }

    public void PlayMonsterAttackAnimation(int monsterId)
    {
        SearchMonsterById(monsterId).PlayAttackAnimation();
    }

    // Utilities ----------------------
    private PlayerController SearchPlayerByName(string playerName)
    {
        return _players.FirstOrDefault(player => player.name == playerName.Substring(0, player.name.Length));
    }

    private Monster SearchMonsterById(int monsterId)
    {
        foreach (var monster in _monsters.Where(monster => monster.id == monsterId))
        {
            return monster;
        }
        Debug.Log($"Monster with id {monsterId} not found.");
        return null;
    }

    private int SearchMonsterIndexById(int monsterId)
    {
        for (int i = 0; i < _monsters.Count; i++)
        {
            if (_monsters[i].id == monsterId)
            {
                return i;
            }
        }

        return -1;
    }

    private int SearchPlayerIndexById(string playerId)
    {
        for (int i = 0; i < _players.Count; i++)
        {
            if (_players[i].id == playerId)
            {
                return i;
            }
        }

        return -1;
    }

    public float RangeFromNearestPlayer(Vector3 pos)
    {
        return Vector3.Distance(GetNearestPlayer(pos).transform.position, pos);
    }

    public PlayerController GetNearestPlayer(Vector3 pos)
    {
        return _players.FindClosest(pos);
    }

    public List<PlayerController> GetNearestPlayers(Vector3 pos, int count)
    {
        return _players.FindClose(pos).TakeWhile(player => count-- >= 0).ToList();
    }

    public float RangeFromNearestMonster(Vector3 pos)
    {
        return Vector3.Distance(_monsters.FindClosest(pos).transform.position, pos);
    }

    public Monster GetNearestMonster(Vector3 pos)
    {
        return _monsters.FindClosest(pos);
    }

    public PlayerController GetPlayer(string id)
    {
        return _players.FirstOrDefault(player => player.id == id);
    }

    public List<T> GetObjectsInRadius<T>(Vector2 point, float r, LayerMask layerMask)
    {
        List<T> temp = new List<T>();
        var colliders = Physics2D.OverlapCircleAll(point, r, layerMask);
        foreach (var col in colliders)
        {
            //Debug.Log(col.name);
            if (col.TryGetComponent(out T obj))
            {
                temp.Add(obj);
            }
        }

        return temp;
    }
}
