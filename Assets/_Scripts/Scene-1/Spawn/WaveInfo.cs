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
    public float monsterHpIncrease;
    public float monsterSpdIncrease;
    public double spawnRate;
    public int spawnersUsedCount;
    public struct WaveSettings
    {
        public float countMultiplier;
        public float hpIncreaseMultiplier;
        public int hpIncreaseEvery;
        public float speedIncreaseMultiplier;
        public int speedIncreaseEvery;
        public int spawnSpeedPercent;
        public int waveCountInfluence;
        public int dirInfluence;

        public WaveSettings(float countMultiplier = 1.2f, float hpIncreaseMultiplier = 1.05f, int hpIncreaseEvery = 3, float speedIncreaseMultiplier = 1.02f, int speedIncreaseEvery = 5, int spawnSpeedPercent = 80, int waveCountInfluence = 2, int dirInfluence = 80)
        {
            this.countMultiplier = countMultiplier;
            this.spawnSpeedPercent = spawnSpeedPercent;
            this.hpIncreaseEvery = hpIncreaseEvery;
            this.hpIncreaseMultiplier = hpIncreaseMultiplier;
            this.speedIncreaseEvery = speedIncreaseEvery;
            this.speedIncreaseMultiplier = speedIncreaseMultiplier;
            this.waveCountInfluence = waveCountInfluence;
            this.dirInfluence = dirInfluence;
        }
    }

    public readonly WaveSettings settings;

    public WaveInfo(int waveNumber, int monsterCount, float baseHpIncrease, float baseSpeedIncrease, WaveSettings settings = new WaveSettings())
    {
        this.waveNumber = waveNumber;
        this.monsterCount = monsterCount;
        monsterHpIncrease = baseHpIncrease;
        monsterSpdIncrease = baseSpeedIncrease;
        this.settings = settings;
        this.settings = new WaveSettings(1.2f, 1.05f, 3, 1.02f, 5, 80, 2, 80);
        CalculateThisWave();
    }

    private void CalculateThisWave()
    {
        spawnRate = Math.Floor(30 / monsterCount * waveNumber *
                               (settings.spawnSpeedPercent - Mathf.Floor(waveNumber / (settings.waveCountInfluence + 1)))) / 100;
        spawnersUsedCount = MiscUtils.IsPrime(monsterCount) ? 1 :
            monsterCount % 4 == 0 ? 4 :
            monsterCount % 3 == 0 ? 3 :
            monsterCount % 2 == 0 ? 2 : Random.Range(1, 5);
    }

    public WaveInfo CalculateNextWave()
    {
        var count = Mathf.FloorToInt(monsterCount + 2 +
                                  waveNumber % (10 + Mathf.FloorToInt(waveNumber / 20)) * settings.countMultiplier);
        //Debug.Log("hpIncreaseEvery:"+_settings.hpIncreaseEvery + ", hp multiplier:" + _settings.hpMultiplier);
        var newMonsterHp = (waveNumber % settings.hpIncreaseEvery == 0 ? 1 + settings.hpIncreaseMultiplier / 100 : 1) * monsterHpIncrease;
        var newBaseSpd = Mathf.FloorToInt(
            (waveNumber % settings.speedIncreaseEvery == 0 ? 1 + settings.speedIncreaseMultiplier / 100 : 1) * monsterSpdIncrease *
            100) / 100;
        var newSpeed = Mathf.Floor(newBaseSpd * (settings.dirInfluence / 4 * spawnersUsedCount + (100 -settings.dirInfluence))/ 100 * 100)/100;
        return new WaveInfo(waveNumber + 1, count, newMonsterHp, newSpeed);
    }

    public Stat CalculateStat(Stat oldStat)
    {
        var newStat = oldStat;
        newStat.hp = Mathf.FloorToInt(oldStat.hp * monsterHpIncrease);
        newStat.movSpd = oldStat.movSpd * monsterSpdIncrease;
        return newStat;
    }
}
