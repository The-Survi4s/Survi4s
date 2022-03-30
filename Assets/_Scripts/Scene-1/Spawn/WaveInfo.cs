using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class WaveInfo
{
    public int waveNumber;
    public int monsterCount;
    public int monsterHp;
    public float monsterSpd;
    public double spawnRate;
    public int spawnersUsedCount;
    public struct WaveSettings
    {
        public float countMultiplier;
        public float hpMultiplier;
        public int hpIncreaseEvery;
        public float speedMultiplier;
        public int speedIncreaseEvery;
        public int spawnSpeedPercent;
        public int waveCountInfluence;

        public WaveSettings(float countMultiplier = 1.2f, float hpMultiplier = 0.05f, int hpIncreaseEvery = 3, float speedMultiplier = 0.02f, int speedIncreaseEvery = 5, int spawnSpeedPercent = 80, int waveCountInfluence = 2)
        {
            this.countMultiplier = countMultiplier;
            this.spawnSpeedPercent = spawnSpeedPercent;
            this.hpIncreaseEvery = hpIncreaseEvery;
            this.hpMultiplier = hpMultiplier;
            this.speedIncreaseEvery = speedIncreaseEvery;
            this.speedMultiplier = speedMultiplier;
            this.waveCountInfluence = waveCountInfluence;
        }
    }

    private WaveSettings _settings;

    public WaveInfo(int waveNumber, int monsterCount, int baseHp, float baseSpeed, WaveSettings settings = new WaveSettings())
    {
        this.waveNumber = waveNumber;
        this.monsterCount = monsterCount;
        monsterHp = baseHp;
        monsterSpd = baseSpeed;
        _settings = settings;
        CalculateThisWave();
    }

    private void CalculateThisWave()
    {
        spawnRate = Math.Floor((30 / monsterCount * waveNumber - 1) *
                               (_settings.spawnSpeedPercent - Mathf.Floor(waveNumber / _settings.waveCountInfluence))) /
                    100;
        spawnersUsedCount = MiscUtils.IsPrime(monsterCount) ? 1 :
            monsterCount % 4 == 0 ? 4 :
            monsterCount % 3 == 0 ? 3 :
            monsterCount % 2 == 0 ? 2 : Random.Range(1, 5);
    }

    public WaveInfo CalculateNextWave()
    {
        var mc = Mathf.FloorToInt(monsterCount + 2 + waveNumber % (10 + Mathf.FloorToInt(waveNumber / 20)) * _settings.countMultiplier);
        return new WaveInfo(waveNumber+1,mc,monsterHp,monsterSpd,new WaveSettings());
    }
}
