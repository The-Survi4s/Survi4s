using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    private readonly List<bool> _occupiedIDs = new List<bool>();
    [SerializeField] private readonly Spawner[] _spawners = new Spawner[4];
    private readonly List<Spawner> _selectedSpawners = new List<Spawner>();

    [SerializeField] private int initialMonsterCount = 3;
    [SerializeField] private float initialSpawnRate = 1;
    [SerializeField] private float monsterHpMultiplier = 1;
    [SerializeField] private float monsterSpdMultiplier = 1;
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

        _currentWaveInfo = new WaveInfo(0, initialMonsterCount, 30, 1);
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
                spawner.SpawnMonster(_monsterPrefabDuplicates[monsterPrefabIndex], id);
            }
        }
    }

    public void OnSendSpawnMonster()
    {
        NetworkClient.Instance.SpawnMonster(_occupiedIDs.Count, GetRandomMonsterType(), GetRandomOrigin(),
            RandomSpawnOffset());
        _occupiedIDs.Add(true);
    }

    private float RandomSpawnOffset()
    {
        throw new NotImplementedException();
    }

    private Monster.Origin GetRandomOrigin() => _selectedSpawners[Random.Range(0, _selectedSpawners.Count)].origin;

    private int GetRandomMonsterType() => Random.Range(0, _monsterPrefabDuplicates.Count);

    public void NextWave()
    {
        _previousWaveInfo = _currentWaveInfo;
        _selectedSpawners.Clear();
        _currentWaveInfo.CalculateNextWave();
        while (_selectedSpawners.Count < _currentWaveInfo.spawnersUsedCount)
        {
            int index = Random.Range(0, _spawners.Length);
            if (!_selectedSpawners.Contains(_spawners[index]))
            {
                _selectedSpawners.Add(_spawners[index]);
            }
        }
    }

    public void ClearIdIndex(int index)
    {
        _occupiedIDs[index] = false;
    }
}
