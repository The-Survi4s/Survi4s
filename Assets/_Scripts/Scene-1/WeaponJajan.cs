using UnityEngine;

public class WeaponJajan : WeaponMelee
{
    public override void OnAttack()
    {
        // Play animation


        if (IsLocal())
        {
            // Detect enemies on range
            Vector2 attackPoint = owner.GetComponent<CharacterWeapon>().GetAttackPoint().position;
            Collider2D[] hitPlayer = GetHitObjectInRange(attackPoint, attackRad, targetMask);

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
    public override void OnCritical(Collider2D[] hitTarget)
    {
        // Buff Player
        foreach (Collider2D x in hitTarget)
        {
            Debug.Log("We buff " + x.name);
        }
    }
}
