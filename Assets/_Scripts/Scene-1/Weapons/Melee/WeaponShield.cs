using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponShield : WeaponMelee
{
    protected override void Update()
    {
        base.Update();
        foreach (var col in GetHitObjectInRange(AttackPoint, attackRad))
        {
            if (col.TryGetComponent(out MonsterBulletBase monsterBullet))
            {
                NetworkClient.Instance.DestroyBullet(monsterBullet.id);
            }
        }
    }
}
