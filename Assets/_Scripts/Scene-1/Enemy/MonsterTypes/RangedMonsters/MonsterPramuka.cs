using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterPramuka : RangedMonsterBase
{
    private Wall _targetWall2;
    private float _evadeDistance = 2;
    private float _evadeCooldown = 2;
    private float _evadeCooldownEndTime;

    protected override void Start()
    {
        var newOrigin = origin switch
        {
            Origin.Right => Origin.Left,
            Origin.Top => Origin.Bottom,
            Origin.Left => Origin.Right,
            Origin.Bottom => Origin.Top,
            _ => origin
        };
        _targetWall2 = TilemapManager.instance.GetWall(newOrigin);
    }

    protected override void Update()
    {
        base.Update();
        if (!NetworkClient.Instance.isMaster) return;
        if(UnitManager.Instance.bulletCount == 0) return;
        var nearestPlayerBullet = UnitManager.Instance.GetNearestBullet(transform.position, true);
        if(!nearestPlayerBullet) return;
        var distanceToNearestPlayerBullet = Vector2.Distance(
            nearestPlayerBullet.transform.position,
            transform.position);
        if (distanceToNearestPlayerBullet < _evadeDistance && _evadeCooldownEndTime < Time.time)
        {
            Evade();
        }
    }

    private void Evade()
    {
        //Pinginnya ke dodge samping tapi aku males jadi teleport ke spawner aja
        Debug.Log("EVADE!");
        transform.SetPositionAndRotation(SpawnManager.instance.GetSpawnerPos(origin), transform.rotation);
        _evadeCooldownEndTime = Time.time + _evadeCooldown;
    }
}
