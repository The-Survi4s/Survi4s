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
    private KdTree<PlayerController> _players = new KdTree<PlayerController>();
    [SerializeField] public List<WeaponBase> weapons;
    private KdTree<Monster> _monsters = new KdTree<Monster>();

    // Eazy Access --------------------------------------------------------------------
    public static UnitManager Instance { get; private set; }

    public int PlayerAliveCount => _players.Count(player => !player.IsDead());
    public int MonsterAliveCount => _monsters.Count;

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
    }

    private void Update()
    {
        _players.UpdatePositions();
        _monsters.UpdatePositions();
    }

    // Spawn Player ------------------------------------------------------------------
    public void SendSpawnPlayer(float x, float y, int skin)
    {
        NetworkClient.Instance.SpawnPlayer(0,0,0);
    }
    public void SpawnPlayer(string idAndName, string id, float x, float y, int skin)
    {
        if (playerPrefab.TryGetComponent(out PlayerController player))
        {
            Vector3 pos = new Vector3(x, y, 0);
            GameObject temp = Instantiate(playerPrefab, pos, Quaternion.identity);
            temp.name = idAndName;
            var p = temp.GetComponent<PlayerController>();
            p.OnPlayerDead += HandlePlayerDead;
            _players.Add(p);
            _players[_players.Count - 1].id = id;
        }
        else
        {
            Debug.Log("Object is not a player!");
        }
    }

    // Deletion
    private void HandlePlayerDead(string id)
    {
        
    }

    private void HandlePlayerDisconnect(string id)
    {

    }

    public void DeleteMonsterFromList(int id)
    {
        _monsters.RemoveAt(SearchMonsterIndexById(id));
    }

    // Spawn Monster -----------------------------------------------------------------
    // Receive
    public void AddMonster(Monster monster)
    {
        monster.SetTargetWall(WallManager.instance.GetRandomWallOn(monster.origin));
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
        SearchPlayerByName(playerName).SyncMousePos(x, y);
    }

    public void SetButton(string playerName, PlayerController.Button button, bool isDown)
    {
        SearchPlayerByName(playerName).SetButton(button, isDown);
    }

    public void OnEquipWeapon(string playerName, string weaponName)
    {
        SearchPlayerByName(playerName).gameObject.GetComponent<PlayerWeaponManager>().OnEquipWeapon(weaponName);
    }

    public void PlayAttackAnimation(string playerName)
    {
        SearchPlayerByName(playerName).gameObject.GetComponent<PlayerWeaponManager>().PlayAttackAnimation();
    }

    public void SpawnBullet(string playerName, float xSpawnPos, float ySpawnPos, float xMousePos, float yMousePos)
    {
        SearchPlayerByName(playerName).gameObject.GetComponent<PlayerWeaponManager>()
            .SpawnBullet(xSpawnPos, ySpawnPos, xMousePos, yMousePos);
    }

    public void ModifyPlayerHp(string playerName, float amount)
    {
        Debug.Log(playerName+" "+amount);
        SearchPlayerByName(playerName).gameObject.GetComponent<CharacterStats>().hitPoint += amount;
    }

    public void CorrectDeadPosition(string playerName, float x, float y)
    {
        SearchPlayerByName(playerName).gameObject.GetComponent<CharacterStats>().CorrectDeadPosition(x, y);
    }

    public void ApplyStatusEffectToMonster(int targetId, StatusEffect statusEffect, int strength, float duration)
    {
        var monster = SearchMonsterById(targetId);
        monster.AddStatusEffect(StatusEffectFactory.CreateNew(monster.rawStat,statusEffect,strength,duration));
    }

    public void PlayMonsterAttackAnimation(int monsterId)
    {
        SearchMonsterById(monsterId).PlayAttackAnimation();
    }

    // Utilities ----------------------
    private PlayerController SearchPlayerByName(string playerName)
    {
        foreach (var player in _players.Where(player => player.gameObject.name == playerName.Substring(0, player.gameObject.name.Length)))
        {
            return player;
        }
        return null;
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

    public float RangeFromNearestPlayer(Vector3 pos)
    {
        return Vector3.Distance(_players.FindClosest(pos).transform.position, pos);
    }

    public PlayerController GetNearestPlayer(Vector3 pos)
    {
        return _players.FindClosest(pos);
    }

    public List<PlayerController> GetNearestPlayers(Vector3 pos, int count)
    {
        return _players.FindClose(pos).TakeWhile(player => count-- >= 0).ToList();
    }

    public List<PlayerController> GetNearestPlayersInRadius(Vector3 pos, float r)
    {
        var collider = Physics.OverlapSphere(pos, r);
        List<PlayerController> temp = new List<PlayerController>();
        foreach (var col in collider)
        {
            if (col.TryGetComponent(out PlayerController player))
            {
                temp.Add(player);
            }
        }

        return temp;
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
    public Collider2D[] GetHitObjectInRange(Vector2 attackPoint, float _attackRad, LayerMask targetLayer)
    {
        return Physics2D.OverlapCircleAll(attackPoint, _attackRad, targetLayer);
    }
}
