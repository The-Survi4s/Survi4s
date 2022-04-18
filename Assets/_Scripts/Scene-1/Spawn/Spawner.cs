using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Spawner : MonoBehaviour
{
    [field: SerializeField] public Monster.Origin origin { get; private set; }
    private float spawnOffset = 10.0f;
    public Vector3 spawnPos
    {
        get
        {
            if (origin == Monster.Origin.Top || origin == Monster.Origin.Bottom)
            {
                return transform.position + new Vector3(spawnOffset, 0, 0);
            }
            else
            {
                return transform.position + new Vector3(0, spawnOffset, 0);
            }
        }
    }

    public void SpawnMonster(GameObject monsterPrefab, int monsterId, float spawnOffset)
    {
        this.spawnOffset = spawnOffset;
        GameObject temp = Instantiate(monsterPrefab, spawnPos, Quaternion.identity);
        Monster monster = temp.GetComponent<Monster>();
        monster.SetIdAndOrigin(origin, monsterId);
        UnitManager.Instance.AddMonster(monster);
    }
}
