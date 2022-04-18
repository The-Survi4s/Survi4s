using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    private List<bool> _occupiedIDs = new List<bool>();
    [SerializeField] private Spawner[] _spawners = new Spawner[4];
    private readonly List<Spawner> _selectedSpawners = new List<Spawner>();

    [Serializable]
    public class InitialMonsterSpawnSetting
    {
        public int count = 3;
        public int hpMultiplier = 1;
        public float speedMultiplier = 1;
        public float randomSpawnOffsetMax = 10;
    }
    [SerializeField] private InitialMonsterSpawnSetting _monsterSpawnSetting = new InitialMonsterSpawnSetting();

    private WaveInfo _previousWaveInfo;
    [SerializeField] private WaveInfo _currentWaveInfo;
    public int currentWave => _currentWaveInfo.waveNumber;

    [Serializable]
    private class MonsterPrefabWeight
    {
        public GameObject monsterPrefab;
        public int weight;
    }

    [SerializeField] private List<MonsterPrefabWeight> _monsterPrefabWeights;
    private List<GameObject> _monsterPrefabDuplicates;

    [SerializeField] private Vector2 _playerSpawnPos;
    [SerializeField] private List<GameObject> playerPrefab;

    public static SpawnManager Instance { get; private set; }

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
        _monsterPrefabDuplicates = new List<GameObject>();

        _currentWaveInfo = new WaveInfo(1, _monsterSpawnSetting.count, _monsterSpawnSetting.hpMultiplier, _monsterSpawnSetting.speedMultiplier);
        _monsterPrefabDuplicates.Clear();
        foreach (var prefabWeight in _monsterPrefabWeights)
        {
            if (prefabWeight.monsterPrefab.TryGetComponent(out Monster monster))
            {
                //Debug.Log("Yes, this is a monster!" + monster.type);
                for (int i = 0; i < prefabWeight.weight; i++)
                {
                    _monsterPrefabDuplicates.Add(prefabWeight.monsterPrefab);
                }
            }
            else
            {
                Debug.Log("The prefab was not a monster! You lied to me!!!");
            }
        }
        //Debug.Log("mpw size:"+_monsterPrefabWeights.Count+", mpd size:"+_monsterPrefabDuplicates.Count);
        SelectSpawners();
    }

    public void SendSpawnPlayer()
    {
        NetworkClient.Instance.SpawnPlayer(_playerSpawnPos, UnitManager.Instance.playerCount);
    }
    public void OnReceiveSpawnPlayer(string idAndName, string id, Vector2 pos, int skin)
    {
        if (playerPrefab[skin].TryGetComponent(out PlayerController player))
        {
            GameObject temp = Instantiate(playerPrefab[skin], pos, Quaternion.identity);
            temp.name = idAndName;
            var p = temp.GetComponent<PlayerController>();
            p.id = id;
            UnitManager.Instance.AddPlayer(p);
        }
        else
        {
            Debug.Log("Object is not a player!");
        }
    }

    public void OnReceiveSpawnMonster(int id, int monsterPrefabIndex, Monster.Origin origin, float spawnOffset)
    {
        //Debug.Log("id:" + id + ", index:" + monsterPrefabIndex + ", size:" + _monsterPrefabDuplicates.Count);
        foreach (Spawner spawner in _spawners)
        {
            if (spawner.origin == origin)
            {
                spawner.SpawnMonster(_monsterPrefabDuplicates[monsterPrefabIndex], id, spawnOffset, _currentWaveInfo);
            }
        }
    }

    private async Task OnSendSpawnMonster(double delayToNext) 
    {
        NetworkClient.Instance.SpawnMonster(_occupiedIDs.Count, GetRandomMonsterType(), GetRandomOrigin(),
            Random.Range(-_monsterSpawnSetting.randomSpawnOffsetMax, _monsterSpawnSetting.randomSpawnOffsetMax));
        _occupiedIDs.Add(true);
        var end = Time.time + delayToNext;
        while (Time.time < end)
        {
            await Task.Yield();
        }
    }

    private Monster.Origin GetRandomOrigin()
    {
        return _selectedSpawners[Random.Range(0, _selectedSpawners.Count)].origin;
    }

    private int GetRandomMonsterType() => Random.Range(0, _monsterPrefabDuplicates.Count);

    public void PrepareNextWave()
    {
        _previousWaveInfo = _currentWaveInfo;
        _currentWaveInfo = _currentWaveInfo.CalculateNextWave();
        SelectSpawners();
    }

    public void PrepareNextWave(WaveInfo nextWaveInfo)
    {
        _previousWaveInfo = _currentWaveInfo;
        _currentWaveInfo = nextWaveInfo;
        _currentWaveInfo = _currentWaveInfo.CalculateNextWave();
        SelectSpawners();
    }

    private void SelectSpawners()
    {
        _selectedSpawners.Clear();
        while (_selectedSpawners.Count < _currentWaveInfo.spawnersUsedCount)
        {
            int index = Random.Range(0, _spawners.Length);
            if (!_selectedSpawners.Contains(_spawners[index]))
            {
                _selectedSpawners.Add(_spawners[index]);
            }
        }
    }

    public async Task StartWave()
    {
        var monsterLeft = _currentWaveInfo.monsterCount;
        while (monsterLeft-- > 0)
        {
            await OnSendSpawnMonster(_currentWaveInfo.spawnRate);
        }
        PrepareNextWave();
    }

    public void ClearIdIndex(int index)
    {
        _occupiedIDs[index] = false;
    }
}
