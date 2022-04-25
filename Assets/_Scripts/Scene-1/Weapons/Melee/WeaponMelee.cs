using UnityEngine;

public abstract class WeaponMelee : WeaponBase
{
    [SerializeField] private float defaultAttackRad;
    public LayerMask targetMask;
    public float attackRad { get; private set; }

    private void Awake()
    {
        attackRad = defaultAttackRad;
    }

    public Collider2D[] GetHitObjectInRange(Vector2 attackPoint, float _attackRad, LayerMask targetLayer)
    {
        return Physics2D.OverlapCircleAll(attackPoint, _attackRad, targetLayer);
    }

    public override void ReceiveAttackMessage()
    {
        // Play animation
        base.ReceiveAttackMessage();
        if (!isLocal) return;
        // Detect enemies on range
        Collider2D[] hitObjects = GetHitObjectInRange(GetOwnerAttackPoint(), attackRad, targetMask);
        if (IsCritical()) OnCritical(hitObjects);
        else OnNormalAttack(hitObjects);
    }

    protected virtual void OnNormalAttack(Collider2D[] targets)
    {
        ModifyAllMonsterHp(targets, -baseAttack);
    }

    protected virtual void OnCritical(Collider2D[] targets)
    {
        ModifyAllMonsterHp(targets, -baseAttack * 2);
    }

    protected void ModifyAllPlayerHp(Collider2D[] targets, float amount)
    {
        foreach (Collider2D target in targets)
        {
            Player player = target.GetComponent<Player>();
            if (player) NetworkClient.Instance.ModifyPlayerHp(player.name, amount);
        }
    }

    protected void ModifyAllMonsterHp(Collider2D[] targets, float amount)
    {
        foreach (Collider2D target in targets)
        {
            Monster monster = target.GetComponent<Monster>();
            Debug.Log($"Monster {monster} get");
            if (monster) NetworkClient.Instance.ModifyMonsterHp(monster.id, amount);
        }
    }

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
