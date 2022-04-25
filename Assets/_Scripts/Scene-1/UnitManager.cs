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
    private KdTree<Player> _playerKdTree;
    private Dictionary<string, Player> _players;
    private List<GameObject> _playerUsernames;
    [SerializeField] public List<WeaponBase> weapons;
    private KdTree<Monster> _monsterKdTree;
    private Dictionary<int, Monster> _monsters;
    private Dictionary<int, BulletBase> _bullets;

    private int _bulletIdCount = 0;

    // Eazy Access --------------------------------------------------------------------
    public static UnitManager Instance { get; private set; }

    public int playerAliveCount => _playerKdTree.Count(player => !player.isDead);
    public int playerCount => _playerKdTree.Count;
    public int monsterAliveCount => _monsterKdTree.Count;

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
        _playerKdTree = new KdTree<Player>(true);
        _monsterKdTree = new KdTree<Monster>();
        _playerUsernames = new List<GameObject>();
        _bullets = new Dictionary<int, BulletBase>();
        _players = new Dictionary<string, Player>();
        _monsters = new Dictionary<int, Monster>();
    }

    private void Update()
    {
        _playerKdTree.UpdatePositions();
        _monsterKdTree.UpdatePositions();

        //Update player username to whatever the hell
    }

    // Spawn ------------------------------------------------------------------
    public void AddPlayer(Player p)
    {
        p.OnPlayerDead += HandlePlayerDead;
        _playerKdTree.Add(p);
        _players.Add(p.name.Trim(), p);

        //Get player username text object from child
    }

    public void AddMonster(Monster monster)
    {
        monster.SetTargetWall(TilemapManager.instance.GetRandomNonDestroyedWallOn(monster.origin));
        _monsterKdTree.Add(monster);
        _monsters.Add(monster.id, monster);
    }

    public int GetIdThenAddBullet(BulletBase bullet)
    {
        _bulletIdCount++;
        _bullets.Add(_bulletIdCount, bullet);
        return _bulletIdCount;
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
        var index = SearchPlayerIndex(_players[idAndName]);
        if (index >= 0) _playerKdTree.RemoveAt(index);
    }

    public void DeleteMonsterFromList(int id)
    {
        var index = SearchMonsterIndex(_monsters[id]);
        if(index >= 0) _monsterKdTree.RemoveAt(index);
    }

    // Send command to units
    // Monster
    public void ModifyMonsterHp(int id, float amount)
    {
        _monsters[id].ModifyHitPoint(amount);
    }

    // Player
    public void SyncMousePos(string playerName, float x, float y)
    {
        var player = _players[playerName];
        if (player) player.SyncMousePos(x, y);
    }

    public void SetButton(string playerName, Player.Button button, bool isDown)
    {
        var player = _players[playerName];
        if (player) player.SetButton(button, isDown);
    }

    public void OnEquipWeapon(string playerName, string weaponName)
    {
        var player = _players[playerName];
        if (player) player.weaponManager.OnEquipWeapon(weaponName);
    }

    public void PlayAttackAnimation(string playerName)
    {
        var player = _players[playerName];
        if (player) player.weaponManager.ReceiveAttackMessage();
    }

    public void SpawnBullet(string playerName, Vector2 spawnPos, Vector2 mousePos)
    {
        var player = _players[playerName];
        if (player) player.weaponManager.SpawnBullet(spawnPos, mousePos);
    }

    public void SpawnBullet(int monsterId, Vector2 spawnPos, Vector2 targetPos)
    {
        var monster = _monsters[monsterId];
        if(monster is RangedMonsterBase rangedMonster) rangedMonster.SpawnBullet(spawnPos, targetPos);
    }

    public void DestroyBullet(int id)
    {
        Destroy(_bullets[id]);
    }

    public void ModifyPlayerHp(string playerName, float amount)
    {
        Debug.Log(playerName + " " + amount);
        var player = _players[playerName];
        if (player) player.stats.hitPoint += amount;
    }

    public void CorrectDeadPosition(string playerName, Vector2 pos)
    {
        var player = _players[playerName];
        if (player) player.stats.CorrectDeadPosition(pos);
    }

    public void ApplyStatusEffectToMonster(int targetId, StatusEffect statusEffect, float duration, int strength)
    {
        var monster = _monsters[targetId];
        if(monster) monster.AddStatusEffect(StatusEffectFactory.CreateNew(monster, statusEffect, duration, strength));
    }

    public void PlayMonsterAttackAnimation(int monsterId)
    {
        var monster = _monsters[monsterId];
        monster.PlayAttackAnimation();
    }

    // Utilities ----------------------

    private int SearchMonsterIndex(Monster monster)
    {
        for (int i = 0; i < _monsterKdTree.Count; i++)
        {
            if (_monsterKdTree[i] == monster)
            {
                return i;
            }
        }

        return -1;
    }

    private int SearchPlayerIndex(Player player)
    {
        for (int i = 0; i < _playerKdTree.Count; i++)
        {
            if (_playerKdTree[i] == player)
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

    public Player GetNearestPlayer(Vector3 pos)
    {
        return _playerKdTree.FindClosest(pos);
    }

    public List<Player> GetNearestPlayers(Vector3 pos, int count)
    {
        return _playerKdTree.FindClose(pos).TakeWhile(player => count-- >= 0).ToList();
    }

    public float RangeFromNearestMonster(Vector3 pos)
    {
        return Vector3.Distance(_monsterKdTree.FindClosest(pos).transform.position, pos);
    }

    public Monster GetNearestMonster(Vector3 pos)
    {
        return _monsterKdTree.FindClosest(pos);
    }

    public Player GetPlayer(string id)
    {
        return _playerKdTree.FirstOrDefault(player => player.id == id);
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
