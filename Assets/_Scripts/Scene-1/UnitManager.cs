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
    private KdTree<Player> _playerAliveKdTree;
    private Dictionary<string, Player> _players;
    private List<GameObject> _playerUsernames;
    [SerializeField] public List<WeaponBase> weapons;
    private KdTree<Monster> _monsterKdTree;
    private Dictionary<int, Monster> _monsters;
    private Dictionary<int, BulletBase> _bullets;

    private int _bulletIdCount = 0;

    // Eazy Access --------------------------------------------------------------------
    public static UnitManager Instance { get; private set; }

    public int playerAliveCount => _playerAliveKdTree.Count();
    public int playerCount => _playerKdTree.Count;
    public int monsterAliveCount => _monsterKdTree.Count;
    public int bulletCount => _bullets.Count;

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
        _playerAliveKdTree = new KdTree<Player>(true);
        _monsterKdTree = new KdTree<Monster>(true);
        _playerUsernames = new List<GameObject>();
        _bullets = new Dictionary<int, BulletBase>();
        _players = new Dictionary<string, Player>();
        _monsters = new Dictionary<int, Monster>();
    }

    private void Update()
    {
        _playerKdTree.UpdatePositions();
        _playerAliveKdTree.UpdatePositions();
        _monsterKdTree.UpdatePositions();

        //Update player username to whatever the hell
    }

    // Spawn ------------------------------------------------------------------
    public void AddPlayer(Player p)
    {
        p.OnPlayerDead += HandlePlayerDead;
        _playerKdTree.Add(p);
        _playerAliveKdTree.Add(p);
        _players.Add(p.name.Trim(), p);

        //Get player username text object from child
    }

    public void AddMonster(Monster monster)
    {
        monster.SetTargetWall(TilemapManager.instance.GetWall(monster.origin));
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
        if(!_players.ContainsKey(idAndName)) return;
        var index = SearchPlayerIndex(_players[idAndName], true);
        if(index >= 0) _playerAliveKdTree.RemoveAt(index);
    }

    public void HandlePlayerDisconnect(string idAndName)
    {
        Debug.Log(idAndName + " disconnected");
        HandlePlayerDead(idAndName);
        if (!_players.ContainsKey(idAndName)) return;

        var index = SearchPlayerIndex(_players[idAndName]);
        if (index >= 0) _playerKdTree.RemoveAt(index);
    }

    public void DeleteMonsterFromList(int id)
    {
        if (!_monsters.ContainsKey(id)) return;
        var index = SearchMonsterIndex(_monsters[id]);
        if(index >= 0) _monsterKdTree.RemoveAt(index);
    }

    public void RemoveBullet(int id)
    {
        _bullets.Remove(id);
    }

    // Send command to units
    // Monster
    public void ModifyMonsterHp(int id, float amount)
    {
        if (!_monsters.ContainsKey(id)) return;
        _monsters[id].ModifyHitPoint(amount);
    }

    // Player
    public void SyncMousePos(string playerName, float x, float y)
    {
        if (!_players.ContainsKey(playerName)) return;
        _players[playerName].SyncMousePos(x, y);
    }

    public void SetButton(string playerName, Player.Button button, bool isDown)
    {
        if (!_players.ContainsKey(playerName)) return;
        _players[playerName].SetButton(button, isDown);
    }

    public void OnEquipWeapon(string playerName, string weaponName)
    {
        if (!_players.ContainsKey(playerName)) return; 
        _players[playerName].weaponManager.OnEquipWeapon(weaponName);
    }

    public void PlayAttackAnimation(string playerName)
    {
        if (!_players.ContainsKey(playerName)) return; 
        _players[playerName].weaponManager.ReceiveAttackMessage();
    }

    public void SpawnBullet(string playerName, Vector2 spawnPos, Vector2 mousePos)
    {
        if (!_players.ContainsKey(playerName)) return;
        _players[playerName].weaponManager.SpawnBullet(spawnPos, mousePos);
    }

    public void SpawnBullet(int monsterId, Vector2 spawnPos, Vector2 targetPos)
    {
        if (!_monsters.ContainsKey(monsterId)) return;
        if(_monsters[monsterId] is RangedMonsterBase rangedMonster) rangedMonster.SpawnBullet(spawnPos, targetPos);
    }

    public void DestroyBullet(int id)
    {
        if(!_bullets.ContainsKey(id)) return;
        Destroy(_bullets[id]);
    }

    public void ModifyPlayerHp(string playerName, float amount)
    {
        if (!_players.ContainsKey(playerName)) return;
        Debug.Log(playerName + " " + amount);
        _players[playerName].stats.hitPoint += amount;
    }

    public void CorrectDeadPosition(string playerName, Vector2 pos)
    {
        if (!_players.ContainsKey(playerName)) return; 
        _players[playerName].stats.CorrectDeadPosition(pos);
    }

    public void ApplyStatusEffectToMonster(int targetId, StatusEffect statusEffect, float duration, int strength)
    {
        if (!_monsters.ContainsKey(targetId)) return; 
        _monsters[targetId].AddStatusEffect(StatusEffectFactory.CreateNew(_monsters[targetId], statusEffect, duration, strength));
    }

    public void PlayMonsterAttackAnimation(int monsterId)
    {
        if (!_monsters.ContainsKey(monsterId)) return;
        _monsters[monsterId].PlayAttackAnimation();
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

    private int SearchPlayerIndex(Player player, bool isAlive)
    {
        if (!isAlive) return SearchPlayerIndex(player);
        for (int i = 0; i < _playerAliveKdTree.Count; i++)
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

    public Player GetNearestPlayer(Vector3 pos, bool isAlive)
    {
        return !isAlive ? GetNearestPlayer(pos) : _playerAliveKdTree.FindClosest(pos);
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

    public BulletBase GetNearestBullet(Vector3 pos)
    {
        var dist = float.MaxValue;
        var nearest = _bullets[0];
        foreach (var bullet in _bullets)
        {
            var dist2 = Vector2.Distance(bullet.Value.transform.position, pos);
            if (dist2 < dist)
            {
                nearest = bullet.Value;
                dist = dist2;
            }
        }

        return nearest;
    }

    public BulletBase GetNearestBullet(Vector3 pos, bool isPlayerOwned)
    {
        var dist = float.MaxValue;
        BulletBase nearest = null;
        foreach (var bullet in _bullets)
        {
            if(!bullet.Value) continue;
            if (!isPlayerOwned || !(bullet.Value is PlayerBulletBase)) continue;
            var dist2 = Vector2.Distance(bullet.Value.transform.position, pos);
            if (dist2 < dist)
            {
                nearest = bullet.Value;
                dist = dist2;
            }
        }

        return nearest;
    }

    public Player GetPlayer(string id)
    {
        return _playerKdTree.FirstOrDefault(player => player.id == id);
    }

    public Player GetLocalPlayer() => _playerKdTree.FirstOrDefault(player => player.isLocal == true);

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
