using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletEsSegar : BulletBase
{
    [SerializeField] private float SplashRad;

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
    }

    protected override void OnNormalShot(Monster monster)
    {
        base.OnNormalShot(monster);
    }

    protected override void OnCriticalShot(Monster monster)
    {
        base.OnCriticalShot(monster);
    }
}