using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponP3K : WeaponMelee
{
    protected override void OnNormalAttack(Collider2D[] targets)
    {
        Heal(targets, baseAttack);
    }

    protected override void OnCritical(Collider2D[] targets)
    {
        Heal(targets, baseAttack * 2);
    }

    private void Heal(Collider2D[] targets, float healAmount)
    {
        List<Collider2D> filteredTargets = new List<Collider2D>();
        foreach (Collider2D col in targets)
        {
            if (col.TryGetComponent(out Player player) && player.isDead) filteredTargets.Add(col);
        }
        Debug.Log("FilteredTargets P3K: " + filteredTargets.Count);
        if (filteredTargets.Count == 0) 
        {
            nextAttackTime = Time.time; //Reset cooldown if fail
        }
        // Heal players
        ModifyHpAll(filteredTargets.ToArray(), healAmount, Target.Player);
        SpawnParticle();
    }
}
