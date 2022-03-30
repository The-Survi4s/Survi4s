using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    private readonly List<bool> _occupiedIDs = new List<bool>();
    [SerializeField] private readonly Spawner[] _spawners = new Spawner[4];
    private readonly List<Spawner> _selectedSpawners = new List<Spawner>();

    [SerializeField] private int initialMonsterCount = 3;
    [SerializeField] private int initialMonsterHp = 30;
    [SerializeField] private float initialMonsterSpeed = 1;
    [SerializeField] private float randomMonsterSpawnOffsetMax = 10;

    private WaveInfo _previousWaveInfo;
    private WaveInfo _currentWaveInfo;
    public int currentWave => _currentWaveInfo.waveNumber;

    [Serializable]
    private class MonsterPrefabWeight
    {
        public GameObject monsterPrefab;
        public int weight;
    }

    [SerializeField] private List<MonsterPrefabWeight> _monsterPrefabWeights;
    private List<GameObject> _monsterPrefabDuplicates;

    private static SpawnManager _instance;
    public static SpawnManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new SpawnManager();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        _monsterPrefabWeights = new List<MonsterPrefabWeight>();
        _monsterPrefabDuplicates = new List<GameObject>();

        _currentWaveInfo = new WaveInfo(0, initialMonsterCount, initialMonsterHp, initialMonsterSpeed);
        _monsterPrefabDuplicates.Clear();
        foreach (var prefabWeight in _monsterPrefabWeights)
        {
            for (int i = 0; i < prefabWeight.weight; i++)
            {
                _monsterPrefabDuplicates.Add(prefabWeight.monsterPrefab);
            }
        }
    }

    public void OnReceiveSpawnMonster(int id, int monsterPrefabIndex, Monster.Origin origin, float spawnOffset)
    {
        foreach (Spawner spawner in _spawners)
        {
            if (spawner.origin == origin)
            {
                spawner.SpawnMonster(_monsterPrefabDuplicates[monsterPrefabIndex], id, spawnOffset);
            }
        }
    }

    public async Task OnSendSpawnMonster(double delayToNext)
    {
        NetworkClient.Instance.SpawnMonster(_occupiedIDs.Count, GetRandomMonsterType(), GetRandomOrigin(),
            Random.Range(-randomMonsterSpawnOffsetMax, randomMonsterSpawnOffsetMax));
        _occupiedIDs.Add(true);
        var end = Time.time + delayToNext;
        while (Time.time < end)
        {
            await Task.Yield();
        }
    }

    private Monster.Origin GetRandomOrigin() => _selectedSpawners[Random.Range(0, _selectedSpawners.Count)].origin;

    private int GetRandomMonsterType() => Random.Range(0, _monsterPrefabDuplicates.Count);

    public void PrepareNextWave()
    {
        _previousWaveInfo = _currentWaveInfo;
        _currentWaveInfo.CalculateNextWave();
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

    public async void StartWave()
    {
        var monsterLeft = _currentWaveInfo.monsterCount;
        var tasks = new List<Task>();
        while (monsterLeft-- > 0)
        {
            tasks.Add(OnSendSpawnMonster(_currentWaveInfo.spawnRate));
        }

        await Task.WhenAll(tasks);
        tasks.Clear();
        PrepareNextWave();
    }

    public void ClearIdIndex(int index)
    {
        _occupiedIDs[index] = false;
    }
}
