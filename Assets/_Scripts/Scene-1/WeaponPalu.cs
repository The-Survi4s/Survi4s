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
            Vector2 attackPoint = owner.GetComponent<CharacterWeapon>().GetAttackPoint().position;
            Collider2D[] hitEnemy = GetHitObjectInRange(attackPoint, attackRad, targetMask);

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
            hitEnemy = GetHitObjectInRange(attackPoint, attackRad, wallMask);
            foreach (Collider2D x in hitEnemy)
            {
                Debug.Log("We repair " + x.name);
            }
        }
    }
    public override void OnCritical(Collider2D[] hitEnemy)
    {
        // Damage them more
        foreach (Collider2D x in hitEnemy)
        {
            Debug.Log("We Crit hit " + x.name);
        }
    }
}
