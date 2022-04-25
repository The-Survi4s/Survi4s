using UnityEngine;

public class WeaponPentungan : WeaponMelee
{
    protected override void OnNormalAttack(Collider2D[] targets)
    {
        base.OnNormalAttack(targets);
        critRate += 10;
        baseAttack += 3;
    }

    protected override void OnCritical(Collider2D[] targets)
    {
        base.OnCritical(targets);
        critRate = defaultCritRate;
    }
}
