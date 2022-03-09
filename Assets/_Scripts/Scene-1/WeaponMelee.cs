using UnityEngine;

public abstract class WeaponMelee : WeaponBase
{
    [SerializeField] private float DefaultAttackRad;
    public LayerMask tergetLayer;
    public float attackRad { get; private set; }


    // Palu 
    // Can repair wall

    // Pentungan
    // Knockback enemy

    // Jajan
    // Area Heal + buff damage

    private void Start()
    {
        attackRad = DefaultAttackRad;
    }

    public override abstract void OnAttack(Vector2 mousePos);
    public override abstract void OnCritical(Vector2 mousePos);

    // Visualy attack ---------------------------------------------------------------
    private void OnDrawGizmosSelected()
    {
        if(owner == null)
        {
            return;
        }
        Vector2 attackPoint = owner.GetComponent<CharacterWeapon>().GetAttackPoint().position;
        Gizmos.DrawSphere(attackPoint, attackRad);
    }
}
