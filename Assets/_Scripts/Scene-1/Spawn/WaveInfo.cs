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
        public int dirInfluence;

        public WaveSettings(float countMultiplier = 1.2f, float hpMultiplier = 1.05f, int hpIncreaseEvery = 3, float speedMultiplier = 1.02f, int speedIncreaseEvery = 5, int spawnSpeedPercent = 80, int waveCountInfluence = 2, int dirInfluence = 80)
        {
            this.countMultiplier = countMultiplier;
            this.spawnSpeedPercent = spawnSpeedPercent;
            this.hpIncreaseEvery = hpIncreaseEvery;
            this.hpMultiplier = hpMultiplier;
            this.speedIncreaseEvery = speedIncreaseEvery;
            this.speedMultiplier = speedMultiplier;
            this.waveCountInfluence = waveCountInfluence;
            this.dirInfluence = dirInfluence;
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
        var count = Mathf.FloorToInt(monsterCount + 2 +
                                  waveNumber % (10 + Mathf.FloorToInt(waveNumber / 20)) * _settings.countMultiplier);
        var newMonsterHp = Mathf.FloorToInt(
            (waveNumber % _settings.hpIncreaseEvery == 0 ? 1 + _settings.hpMultiplier / 100 : 1) * monsterHp);
        var newBaseSpd = Mathf.FloorToInt(
            (waveNumber % _settings.speedIncreaseEvery == 0 ? 1 + _settings.speedMultiplier / 100 : 1) * monsterSpd *
            100) / 100;
        var newSpeed = Mathf.Floor(newBaseSpd * (_settings.dirInfluence / 4 * spawnersUsedCount + (100 -_settings.dirInfluence))/ 100 * 100)/100;
        return new WaveInfo(waveNumber+1,count,newMonsterHp,newSpeed);
    }
}
