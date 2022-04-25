using System;
using UnityEngine;

public class WeaponJajan : WeaponMelee
{
    protected override void OnNormalAttack(Collider2D[] targets)
    {
        // Heal players
        ModifyAllPlayerHp(targets, baseAttack);
        SpawnParticle();
    }

    protected override void OnCritical(Collider2D[] targets)
    {
        ModifyAllPlayerHp(targets, baseAttack * 3);
        SpawnParticle();
    }

    protected override void SpawnParticle()
    {
        //
    }
}
