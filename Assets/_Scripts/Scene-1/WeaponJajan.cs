using UnityEngine;

public class WeaponJajan : WeaponMelee
{
    public override void OnAttack(Vector2 mousePos)
    {
        // Play animation

        // Detect enemies on range
        Vector2 attackPoint = owner.GetComponent<CharacterWeapon>().GetAttackPoint().position;
        Collider2D[] hitPlayer = Physics2D.OverlapCircleAll(attackPoint, attackRad, tergetLayer);

        // Damage them
        foreach (Collider2D x in hitPlayer)
        {
            Debug.Log("We heal " + x.name);
        }
    }
    public override void OnCritical(Vector2 mousePos)
    {
        // Play animation

        // Detect enemies on range
        Vector2 attackPoint = owner.GetComponent<CharacterWeapon>().GetAttackPoint().position;
        Collider2D[] hitPlayer = Physics2D.OverlapCircleAll(attackPoint, attackRad, tergetLayer);

        // Damage them
        foreach (Collider2D x in hitPlayer)
        {
            Debug.Log("We buff " + x.name);
        }
    }
}
