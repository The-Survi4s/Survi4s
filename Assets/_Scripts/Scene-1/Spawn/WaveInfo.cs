using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveInfo : MonoBehaviour
{
    public int waveNumber;
    public int monsterCount;
    public float spawnRate;
    public int spawnersUsedCount;

    public void Init(int monsterCount, float spawnRate, int spawnersUsed)
    {
        waveNumber = 0;
        this.monsterCount = monsterCount;
        this.spawnRate = spawnRate;
        spawnersUsedCount = spawnersUsed;
    }

    public void CalculateNextWave()
    {
        
        throw new System.NotImplementedException();
    }
}
