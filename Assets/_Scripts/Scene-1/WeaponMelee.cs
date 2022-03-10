using UnityEngine;

public abstract class WeaponMelee : WeaponBase
{
    [SerializeField] private float DefaultAttackRad;
    public LayerMask targetMask;
    public float attackRad { get; private set; }

    private void Start()
    {
        attackRad = DefaultAttackRad;
    }

    public override abstract void OnAttack();
    public override abstract void OnCritical(Collider2D[] hitEnemy);

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
