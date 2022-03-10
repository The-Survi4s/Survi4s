using UnityEngine;

public class WeaponPalu : WeaponMelee
{
    [SerializeField] private LayerMask wallMask;

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

            // Special ability
            hitEnemy = GetHitObjectInRange(GetOwnerAttackPoint(), attackRad, wallMask);
            foreach (Collider2D x in hitEnemy)
            {
                Debug.Log("We repair " + x.name);
            }
        }
    }
    private void OnCritical(Collider2D[] hitEnemy)
    {
        // Damage them more
        foreach (Collider2D x in hitEnemy)
        {
            Debug.Log("We Crit hit " + x.name);
        }
    }
}
