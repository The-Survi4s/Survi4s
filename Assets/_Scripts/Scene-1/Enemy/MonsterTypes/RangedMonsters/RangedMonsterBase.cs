using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedMonsterBase : Monster
{
    [SerializeField] private GameObject _bulletPrefab;

    protected override void Attack(Component nearestObj)
    {
        NetworkClient.Instance.SpawnBullet(transform.position, nearestObj.transform.position, id);
    }

    public void SpawnBullet(Vector2 spawnPos, Vector2 targetPos)
    {
        // Make a bullet
        GameObject temp = Instantiate(_bulletPrefab, spawnPos, Quaternion.identity);
        MonsterBulletBase bulTemp = temp.GetComponent<MonsterBulletBase>();

        // init bullet
        bulTemp.Init(this, targetPos, UnitManager.Instance.GetIdThenAddBullet(bulTemp));
    }
}
