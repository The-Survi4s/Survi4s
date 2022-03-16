using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> monsterPrefabs;
    private List<bool> occupiedIDs = new List<bool>();
    [SerializeField] private Spawner[] _spawners = new Spawner[4];
    private List<Spawner> selectedSpawners = new List<Spawner>();

    public class WaveInfo
    {
        public int waveNumber { get; }
        public int monsterCount { get; }
        public float spawnRate { get; }
        public int spawnersUsedCount { get; }
    }

    [SerializeField] private List<WaveInfo> _waveInfos;
    public int currentWave { get; private set; }

    private static SpawnManager _instance;
    public static SpawnManager Instance
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

    public void OnReceiveSpawnMonster(int ID, Monster.Type type, Monster.Origin origin)
    {
        foreach (Spawner spawner in _spawners)
        {
            if (spawner.origin == origin)
            {
                spawner.SpawnMonster(monsterPrefabs[(int)type], ID);
            }
        }
    }

    public void OnSendSpawnMonster()
    {
        
        NetworkClient.Instance.SpawnMonster(occupiedIDs.Count, GetRandomMonsterType(), GetRandomOrigin());
        occupiedIDs.Add(true);
    }

    private Monster.Origin GetRandomOrigin()
    {
        return selectedSpawners[Random.Range(0, selectedSpawners.Count - 1)].origin;
    }

    private Monster.Type GetRandomMonsterType()
    {
        return Monster.Type.TypeA; // BELUM RANDOM -------------------------------------------------
    }

    public void NextWave()
    {
        selectedSpawners.Clear();
        currentWave++;
        while (selectedSpawners.Count < _waveInfos[currentWave].spawnersUsedCount)
        {
            int index = Random.Range(0, _spawners.Length - 1);
            if (!selectedSpawners.Contains(_spawners[index]))
            {
                selectedSpawners.Add(_spawners[index]);
            }
        }
    }

    public void ClearIDIndex(int index)
    {
        occupiedIDs[index] = false;
    }
}
