using UnityEngine;

public class WeaponJajan : WeaponMelee
{
    public override void OnAttack()
    {
        // Play animation


        if (IsLocal())
        {
            // Detect enemies on range
            Collider2D[] hitPlayer = GetHitObjectInRange(GetOwnerAttackPoint(), attackRad, targetMask);

            // Heal player
            foreach (Collider2D x in hitPlayer)
            {
                Debug.Log("We Heal " + x.name);
            }

            // Calculate crit
            if (IsCrit())
            {
                // Call crit skill
                OnCritical(hitPlayer);
            }
        }
    }
    private void OnCritical(Collider2D[] hitTarget)
    {
        // Buff Player
        foreach (Collider2D x in hitTarget)
        {
            Debug.Log("We buff " + x.name);
        }
    }
}
