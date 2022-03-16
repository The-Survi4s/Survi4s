using UnityEngine;

public class WeaponPentungan : WeaponMelee
{
    protected override void OnCritical(Collider2D[] targets)
    {
        // Damage them
        foreach (Collider2D x in targets)
        {
            Debug.Log("We hit " + x.name);

            // Special ability
            Debug.Log("We push back " + x.name);
        }
        base.OnCritical(targets);
    }
}
