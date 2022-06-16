using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponJajan : WeaponMelee
{
    protected override void OnNormalAttack(Collider2D[] targets)
    {
        JajanEat(targets, baseAttack);
    }

    protected override void OnCritical(Collider2D[] targets)
    {
        JajanEat(targets, baseAttack * 3);
    }

    private void JajanEat(Collider2D[] targets, float healAmount)
    {
        List<Collider2D> filteredTargets = new List<Collider2D>();
        foreach (Collider2D col in targets)
        {
            if (col.TryGetComponent(out Player player) && !player.isDead) filteredTargets.Add(col);
        }
        // Heal players
        ModifyHpAll(filteredTargets.ToArray(), healAmount, Target.Player);
        SpawnParticle();
    }
}
