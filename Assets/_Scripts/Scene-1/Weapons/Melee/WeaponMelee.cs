using UnityEngine;

public abstract class WeaponMelee : WeaponBase
{
    [SerializeField] private float defaultAttackRad;
    public LayerMask targetMask;
    public float attackRad { get; private set; }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        attackRad = defaultAttackRad;
    }

    /*private void LateUpdate()
    {
        if (GetComponent<Animator>().enabled == true)
            transform.localEulerAngles = new Vector3(0, 0, currentZRot + transform.localEulerAngles.z);
        else
            transform.localEulerAngles = transform.localEulerAngles;
    }*/

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

        PlayAnimation();
        //currentZRot = transform.localEulerAngles.z;
    }

    protected virtual void OnCritical(Collider2D[] targets)
    {
        ModifyAllMonsterHp(targets, -baseAttack * 2);

        PlayAnimation();
        //currentZRot = transform.localEulerAngles.z;
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
            //Debug.Log($"Monster {monster} get");
            if (monster) NetworkClient.Instance.ModifyMonsterHp(monster.id, amount);
        }
    }

    // Visually attack ---------------------------------------------------------------
    private void OnDrawGizmosSelected()
    {
        if(!ownerPlayer)
        {
            return;
        }
        Vector2 attackPoint = ownerPlayer.weaponManager.GetAttackPoint().position;
        Gizmos.DrawSphere(attackPoint, attackRad);
    }

    protected override void PlayAnimation()
    {
        base.PlayAnimation();
        
        if(_animator != null)
            _animator.Play("Attack");
    }

    /*protected override void PlayAnimation()
    {
        base.PlayAnimation();

        //GetComponent<Animator>().speed = 1f;
        GetComponent<Animator>().enabled = true;
        GetComponent<Animator>().Play("Swing_Melee");
    }

    void StopAnimation()
    {
        GetComponent<Animator>().enabled = false;
        Debug.Log("STOP");
    }*/
}
