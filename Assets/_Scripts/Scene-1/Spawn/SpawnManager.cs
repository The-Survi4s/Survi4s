using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    private AudioManager audioManager; 

    private readonly List<bool> _occupiedIDs = new List<bool>();
    private readonly List<Spawner> _spawners = new List<Spawner>();
    private readonly List<Spawner> _selectedSpawners = new List<Spawner>();

    /// <summary>
    /// Class used as the initial setting for wave spawns
    /// </summary>
    [Serializable]
    public class InitialMonsterSpawnSetting
    {
        [Min(0)]
        public int count = 3;
        [Min(0.1f)]
        public float hpMultiplier = 1;
        [Min(0)]
        public float speedMultiplier = 1;
        public float randomSpawnOffsetMax = 10;
    }
    /// <summary>
    /// First wave's monster spawn settings
    /// </summary>
    [SerializeField] private InitialMonsterSpawnSetting _monsterSpawnSetting;

    private WaveInfo _previousWaveInfo;
    [SerializeField] private WaveInfo _currentWaveInfo;
    public int currentWave => _currentWaveInfo.waveNumber;

    /// <summary>
    /// Class to store the weight value of a <see cref="GameObject"/>
    /// </summary>
    [Serializable]
    private class PrefabWeight
    {
        public GameObject prefab;
        [Min(1)]
        public int weight;
    }

    /// <summary>
    /// Assign in inspector. This list stores weight values of each monster type assigned.
    /// <br/><br/>Used to populate <see cref="_monsterPrefabDuplicates"/>
    /// </summary>
    [SerializeField] private List<PrefabWeight> _monsterPrefabWeights;
    /// <summary>
    /// A list to store the prefab duplicates of each <see cref="Monster.Type"/>s. Each <see cref="Monster.Type"/> is duplicated by the value of it's weight
    /// <br/>Select from this list to get a random monster. Chances to get a specific <see cref="Monster.Type"/> is based on it's weight compared to others
    /// </summary>
    private List<GameObject> _monsterPrefabDuplicates;

    [SerializeField] private Vector2 _playerSpawnPos = new Vector2(2, 2);
    [SerializeField] private List<GameObject> _playerPrefab;

    public static SpawnManager instance { get; private set; }

    private void Awake()
    {
        audioManager = GetComponent<AudioManager>();

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        _monsterPrefabDuplicates = new List<GameObject>();
        _currentWaveInfo = new WaveInfo(1, _monsterSpawnSetting.count, _monsterSpawnSetting.hpMultiplier, _monsterSpawnSetting.speedMultiplier);
        
        PopulateMonsterDuplicates();
    }

    /// <summary>
    /// Populates <see cref="_monsterPrefabDuplicates"/> with prefabs from <see cref="_monsterPrefabWeights"/> times it's weight
    /// </summary>
    private void PopulateMonsterDuplicates()
    {
        _monsterPrefabDuplicates.Clear();
        foreach (var prefabWeight in _monsterPrefabWeights)
        {
            if (prefabWeight.prefab.TryGetComponent(out Monster monster))
            {
                for (int i = 0; i < prefabWeight.weight; i++)
                {
                    _monsterPrefabDuplicates.Add(prefabWeight.prefab);
                }
            }
            else
            {
                Debug.Log("The prefab was not a monster! You lied to me!!!");
            }
        }
    }

    public void AddSpawner(Spawner spawner) => _spawners.Add(spawner);

    /// <summary>
    /// Sends a message to server to spawn player
    /// </summary>
    public void SendSpawnPlayer() => NetworkClient.Instance.SpawnPlayer(_playerSpawnPos, UnitManager.Instance.playerCount);

    /// <summary>
    /// Called by <see cref="NetworkClient"/>. Instantiates a <see cref="Player"/> on <see cref="_playerPrefab"/>
    /// </summary>
    /// <param name="idAndName">Player's id and name</param>
    /// <param name="id">the id of the player</param>
    /// <param name="pos">the position the <see cref="Player"/> will spawn in</param>
    /// <param name="skin">Which sprite will the <see cref="Player"/> use</param>
    public void ReceiveSpawnPlayer(string idAndName, int id, Vector2 pos, int skin)
    {
        if (_playerPrefab[skin].TryGetComponent(out Player player))
        {
            GameObject temp = Instantiate(_playerPrefab[skin], pos, Quaternion.identity);
            temp.name = idAndName.Trim();
            var p = temp.GetComponent<Player>();
            p.id = id;
            UnitManager.Instance.AddPlayer(p);
        }
        else
        {
            Debug.Log("Object is not a player!");
        }
    }

    /// <summary>
    /// Called by <see cref="NetworkClient"/> to spawn a <see cref="Monster"/>
    /// </summary>
    /// <param name="id"><see cref="Monster"/>'s id</param>
    /// <param name="index">The prefab index of <see cref="_monsterPrefabDuplicates"/> to instantiate</param>
    /// <param name="origin"><see cref="Monster"/>'s <see cref="Origin"/></param>
    /// <param name="spawnOffset">The offset to spawn <see cref="Monster"/> from <see cref="Spawner"/>'s center</param>
    public void ReceiveSpawnMonster(int id, int index, Origin origin, float spawnOffset)
    {
        //Debug.Log("Receive spawn id:" + id + ", index:" + index + ", size:" + _monsterPrefabDuplicates.Count);
        var spawners = _spawners.Where(spawner => spawner.origin == origin).ToList();
        spawners[Random.Range(0, spawners.Count)].SpawnMonster(_monsterPrefabDuplicates[index], id, spawnOffset, _currentWaveInfo);

        int rand = Random.Range(0, audioManager.sounds.Length + 1);
        if (rand == 0)
            audioManager.Play("Spawn_Steam");
        else if (rand == 1)
            audioManager.Play("Spawn_Crack");
        else if (rand == 2)
            audioManager.Play("Spawn_Roar");
    }

    /// <summary>
    /// Sends a message to server every <paramref name="interval"/> to spawn <see cref="Monster"/>
    /// </summary>
    /// <param name="interval">time interval in miliseconds</param>
    /// <returns></returns>
    private async Task SendSpawnMonster(double interval) 
    {
        NetworkClient.Instance.SpawnMonster(GetVacantId(), GetRandomMonsterType(), GetRandomOrigin(),
            Random.Range(-_monsterSpawnSetting.randomSpawnOffsetMax, _monsterSpawnSetting.randomSpawnOffsetMax));
        var end = Time.time + interval;
        while (Time.time < end)
        {
            await Task.Yield();
        }


    }

    /// <summary>
    /// Gets a vacant id in the monster id pool (<see cref="_occupiedIDs"/>)
    /// </summary>
    /// <returns></returns>
    private int GetVacantId()
    {
        for (var index = 0; index < _occupiedIDs.Count; index++)
        {
            if (!_occupiedIDs[index])
            {
                _occupiedIDs[index] = true;
                //Debug.Log("Vacant id: " + index);
                return index;
            }
        }
        _occupiedIDs.Add(true);
        //Debug.Log("New id: " + (_occupiedIDs.Count - 1));
        return _occupiedIDs.Count - 1;
    }

    /// <summary>
    /// Gets the <see cref="Origin"/> of a random <see cref="_selectedSpawners"/>
    /// </summary>
    /// <returns></returns>
    private Origin GetRandomOrigin()
    {
        if(_selectedSpawners.Count == 0) SelectSpawners();
        return _selectedSpawners[Random.Range(0, _selectedSpawners.Count)].origin;
    }

    /// <summary>
    /// Gets a random index of <see cref="_monsterPrefabDuplicates"/>.
    /// <br/>Chance of getting a specific type is based on it's weight compared to others
    /// </summary>
    /// <returns></returns>
    private int GetRandomMonsterType() => Random.Range(0, _monsterPrefabDuplicates.Count);

    /// <summary>
    /// Calculates next <see cref="WaveInfo"/> and selects random <see langword="n"/> spawners.
    /// </summary>
    public void PrepareNextWave()
    {
        _previousWaveInfo = _currentWaveInfo;
        _currentWaveInfo = _currentWaveInfo.CalculateNextWave();
        SelectSpawners();
    }

    /// <summary>
    /// Sets the next <see cref="WaveInfo"/> to <paramref name="nextWaveInfo"/>,
    /// calculates it, then selects random <see langword="n"/> spawners.
    /// </summary>
    /// <param name="nextWaveInfo"></param>
    public void PrepareNextWave(WaveInfo nextWaveInfo)
    {
        _previousWaveInfo = _currentWaveInfo;
        _currentWaveInfo = nextWaveInfo;
        _currentWaveInfo = _currentWaveInfo.CalculateNextWave();
        SelectSpawners();
    }

    /// <summary>
    /// Selects random <see langword="_currentWaveInfo.spawnersUsedCount"/> spawners to use for next wave. 
    /// </summary>
    private void SelectSpawners()
    {
        _selectedSpawners.Clear();
        while (_selectedSpawners.Count < _currentWaveInfo.spawnersUsedCount)
        {
            int index = Random.Range(0, _spawners.Count);
            if (!_selectedSpawners.Contains(_spawners[index]))
            {
                _selectedSpawners.Add(_spawners[index]);
            }
        }
    }

    /// <summary>
    /// Starts the wave. Requires <see cref="_currentWaveInfo"/> to not be <see langword="null"/>
    /// </summary>
    /// <returns></returns>
    public async Task StartWave()
    {
        if(_currentWaveInfo == null) return;
        var monsterLeft = _currentWaveInfo.monsterCount;
        while (monsterLeft-- > 0)
        {
            await SendSpawnMonster(_currentWaveInfo.spawnRate);
        }
        PrepareNextWave();
    }

    public void ClearIdIndex(int index) => _occupiedIDs[index] = false;

    /// <summary>
    /// Gets the <see cref="Vector3"/> position of spawner on <paramref name="origin"/>. <br/>
    /// If there's more than 1, pick random. If none, ignores <paramref name="origin"/>. 
    /// </summary>
    /// <param name="origin"></param>
    /// <returns></returns>
    public Vector3 GetSpawnerPos(Origin origin)
    {
        var spawnersOnOrigin = _spawners.Where(spawner => spawner.origin == origin).ToArray();
        if (spawnersOnOrigin.Length == 0) spawnersOnOrigin = _spawners.ToArray();
        int index = Random.Range(0, spawnersOnOrigin.Length);
        return spawnersOnOrigin[index].transform.position;
    }
}
