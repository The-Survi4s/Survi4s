using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponP3K : WeaponMelee
{
    protected override void OnNormalAttack(Collider2D[] targets)
    {
        List<Collider2D> filteredTargets = new List<Collider2D>();
        foreach (Collider2D col in targets)
        {
            if (col.TryGetComponent(out Player player) && player.isDead) filteredTargets.Add(col);
        }
        // Heal players
        ModifyHpAll(filteredTargets.ToArray(), baseAttack, Target.Player);
        SpawnParticle();
    }
}
