using UnityEngine;

public class WeaponPentungan : WeaponMelee
{
    protected override void OnNormalAttack(Collider2D[] targets)
    {
        base.OnNormalAttack(targets);
        critRate += 10;
        baseAttack += 2;
    }

    protected override void OnCritical(Collider2D[] targets)
    {
        critRate = defaultCritRate;
        base.OnCritical(targets);
        baseAttack = defaultBaseAttack;
    }
}
