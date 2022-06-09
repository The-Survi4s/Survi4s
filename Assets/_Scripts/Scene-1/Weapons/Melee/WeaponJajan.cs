using System;
using UnityEngine;

public class WeaponJajan : WeaponMelee
{
    protected override void OnNormalAttack(Collider2D[] targets)
    {
        // Heal players
        ModifyHpAll(targets, baseAttack, Target.Player);
        SpawnParticle();
    }

    protected override void OnCritical(Collider2D[] targets)
    {
        ModifyHpAll(targets, baseAttack * 3, Target.Player);
        SpawnParticle();
    }

    protected override void SpawnParticle()
    {
        //
    }
}
