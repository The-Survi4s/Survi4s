using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Spawner : MonoBehaviour
{
    public Monster.Origin origin { get; private set; }
    private float _spawnOffset = 10.0f;
    public Vector3 spawnPos
    {
        get
        {
            if (origin == Monster.Origin.Top || origin == Monster.Origin.Bottom)
            {
                return transform.position + new Vector3(_spawnOffset, 0, -1);
            }
            else
            {
                return transform.position + new Vector3(0, _spawnOffset, -1);
            }
        }
    }

    private void Start()
    {
        origin = WallManager.instance.GetOriginFromWorldPos(transform.position);
        SpawnManager.Instance.AddSpawner(this);
        this.gameObject.name = "Spawner " + origin;
    }

    public void SpawnMonster(GameObject monsterPrefab, int monsterId, float spawnOffset, WaveInfo waveInfo)
    {
        this._spawnOffset = spawnOffset;
        GameObject temp = Instantiate(monsterPrefab, spawnPos, Quaternion.identity);
        Monster monster = temp.GetComponent<Monster>();
        monster.Initialize(origin, monsterId, waveInfo.CalculateStat(monster.defaultStat));
        UnitManager.Instance.AddMonster(monster);
    }
}
