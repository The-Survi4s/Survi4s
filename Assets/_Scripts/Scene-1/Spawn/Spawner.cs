using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class Spawner : MonoBehaviour
{
    public Origin origin { get; private set; }
    private float _spawnOffset = 10.0f;
    public Vector3 spawnPos
    {
        get
        {
            if (origin == Origin.Top || origin == Origin.Bottom)
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
        origin = TilemapManager.instance.GetOrigin(transform.position);
        SpawnManager.instance.AddSpawner(this);
        this.gameObject.name = "Spawner " + origin;
    }

    public void SpawnMonster(GameObject monsterPrefab, int monsterId, float spawnOffset, WaveInfo waveInfo)
    {
        //Debug.Log("Id " + monsterId + " exists? " + UnitManager.Instance.MonsterIdExist(monsterId));
        this._spawnOffset = spawnOffset;
        GameObject temp = Instantiate(monsterPrefab, spawnPos, Quaternion.identity);
        Monster monster = temp.GetComponent<Monster>();
        monster.Initialize(origin, monsterId, waveInfo.CalculateStat(monster.defaultStat));
        UnitManager.Instance.AddMonster(monster);
    }
}
