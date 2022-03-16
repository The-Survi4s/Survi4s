using UnityEngine;

public class WeaponJajan : WeaponMelee
{
    protected override void OnNormalAttack(Collider2D[] targets)
    {
        // Heal players
        foreach (Collider2D target in targets)
        {
            Debug.Log("We Heal " + target.name);
        }

        SpawnParticle();
    }

    protected override void OnCritical(Collider2D[] targets)
    {
        // Buff players
        foreach (Collider2D target in targets)
        {
            Debug.Log("We buff " + target.name);
        }
    }

    protected override void SpawnParticle()
    {
        //
    }
}
