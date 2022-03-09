using UnityEngine;

public class WeaponPalu : WeaponMelee
{
    [SerializeField] private LayerMask wallMask;

    public override void OnAttack(Vector2 mousePos)
    {
        // Play animation

        // Detect enemies on range
        Vector2 attackPoint = owner.GetComponent<CharacterWeapon>().GetAttackPoint().position;
        Collider2D[] hitEnemy = Physics2D.OverlapCircleAll(attackPoint, attackRad, tergetLayer);

        // Damage them
        foreach(Collider2D x in hitEnemy)
        {
            Debug.Log("We hit " + x.name);
        }

        // Special ability
        hitEnemy = Physics2D.OverlapCircleAll(attackPoint, attackRad, wallMask);
        foreach (Collider2D x in hitEnemy)
        {
            Debug.Log("We repair " + x.name);
        }
    }
    public override void OnCritical(Vector2 mousePos)
    {
        // Play animation

        // Detect enemies on range
        Vector2 attackPoint = owner.GetComponent<CharacterWeapon>().GetAttackPoint().position;
        Collider2D[] hitEnemy = Physics2D.OverlapCircleAll(attackPoint, attackRad, tergetLayer);

        // Damage them
        foreach (Collider2D x in hitEnemy)
        {
            Debug.Log("We hit " + x.name);
        }

        // Special ability
        hitEnemy = Physics2D.OverlapCircleAll(attackPoint, attackRad, wallMask);
        foreach (Collider2D x in hitEnemy)
        {
            Debug.Log("We repair " + x.name);
        }
    }
}
