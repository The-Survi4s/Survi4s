using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletWaterBalloon : PlayerBulletBase
{
    [SerializeField] private float _splashRad;
    [SerializeField] private LayerMask _layerMask;

    protected override void OnNormalShot(Monster monster)
    {
        DamageMonstersInArea(monster.transform.position, base.OnNormalShot);
    }

    protected override void OnCriticalShot(Monster monster)
    {
        DamageMonstersInArea(monster.transform.position, base.OnCriticalShot);
    }

    private void DamageMonstersInArea(Vector2 hitMonsterPos, Action<Monster> doSomethingWithTheMonster)
    {
        var monsters = UnitManager.Instance.GetObjectsInRadius<Monster>(hitMonsterPos, _splashRad, _layerMask);
        foreach (var monster in monsters)
        {
            Debug.Log("Splash damage " + monster);
            doSomethingWithTheMonster?.Invoke(monster);
        }
    }
}