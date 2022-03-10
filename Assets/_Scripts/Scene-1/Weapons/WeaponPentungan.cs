using UnityEngine;

public class WeaponPentungan : WeaponMelee
{
    public override void OnAttack()
    {
        // Play animation


        if (IsLocal())
        {
            // Detect enemies on range
            Collider2D[] hitEnemy = GetHitObjectInRange(GetOwnerAttackPoint(), attackRad, targetMask);

            // Calculate crit
            if (IsCrit())
            {
                // Call crit attack
                OnCritical(hitEnemy);
            }
            else
            {
                // Call normal attack
                foreach (Collider2D x in hitEnemy)
                {
                    Debug.Log("We hit " + x.name);
                }
            }
        }
    }
    private void OnCritical(Collider2D[] hitEnemy)
    {
        // Damage them
        foreach (Collider2D x in hitEnemy)
        {
            Debug.Log("We hit " + x.name);

            // Special ability
            Debug.Log("We push back " + x.name);
        }
    }
}
