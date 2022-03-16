using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTipeX : BulletBase
{
    protected override void OnCriticalShot(Monster monster)
    {
        base.OnCriticalShot(monster);
        NetworkClient.Instance.StunMonster(monster.ID, monster.origin, weapon.baseAttack);
    }
}
