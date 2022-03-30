using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [field: SerializeField] public Monster.Origin origin { get; private set; }
    [SerializeField] private float randomSpawnOffset = 10.0f;
    public Vector3 spawnPos
    {
        get
        {
            if (origin == Monster.Origin.Top || origin == Monster.Origin.Bottom)
            {
                return transform.position + new Vector3(Random.Range(-randomSpawnOffset, randomSpawnOffset), 0, 0);
            }
            else
            {
                return transform.position + new Vector3(0, Random.Range(-randomSpawnOffset, randomSpawnOffset), 0);
            }
        }
    }

    public void SpawnMonster(GameObject monsterPrefab, int monsterID)
    {
        GameObject temp = Instantiate(monsterPrefab, spawnPos, Quaternion.identity);
        Monster monster = temp.GetComponent<Monster>();
        monster.Init(origin, monsterID);
        UnitManager.Instance.AddMonster(monster);
    }
}
