using UnityEngine;

public abstract class WeaponMelee : WeaponBase
{
    [SerializeField] private float DefaultAttackRad;
    public LayerMask targetMask;
    public float attackRad { get; private set; }

    private void Awake()
    {
        attackRad = DefaultAttackRad;
    }
    public Collider2D[] GetHitObjectInRange(Vector2 attackPoint, float _attackRad, LayerMask targetLayer)
    {
        return Physics2D.OverlapCircleAll(attackPoint, attackRad, targetLayer);
    }

    public override void OnAttack()
    {
        // Play animation
        base.OnAttack();

        if (!IsLocal()) return;
        // Detect enemies on range
        Collider2D[] hitObjects = GetHitObjectInRange(GetOwnerAttackPoint(), attackRad, targetMask);
        if (IsCritical()) OnCritical(hitObjects);
        else OnNormalAttack(hitObjects);
    }

    protected virtual void OnNormalAttack(Collider2D[] targets){}
    protected virtual void OnCritical(Collider2D[] targets){}

    // Visually attack ---------------------------------------------------------------
    private void OnDrawGizmosSelected()
    {
        if(owner == null)
        {
            return;
        }
        Vector2 attackPoint = owner.GetComponent<PlayerWeaponManager>().GetAttackPoint().position;
        Gizmos.DrawSphere(attackPoint, attackRad);
    }
}