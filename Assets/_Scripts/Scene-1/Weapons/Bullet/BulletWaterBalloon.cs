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
        DamageMonstersInArea(monster, base.OnNormalShot);
    }

    protected override void OnCriticalShot(Monster monster)
    {
        DamageMonstersInArea(monster, base.OnCriticalShot);
    }

    private void DamageMonstersInArea(Monster monster, Action<Monster> doSomethingWithTheMonster)
    {
        doSomethingWithTheMonster?.Invoke(monster);
        var monsters = UnitManager.Instance.GetObjectsInRadius<Monster>(monster.transform.position, _splashRad, _layerMask);
        foreach (var m in monsters)
        {
            Debug.Log("Splash damage " + m);
            doSomethingWithTheMonster?.Invoke(m);
        }
    }
}