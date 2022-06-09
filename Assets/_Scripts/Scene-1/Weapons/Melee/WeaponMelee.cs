using UnityEngine;

public abstract class WeaponMelee : WeaponBase
{
    [SerializeField] private LayerMask _targetMask;
    [SerializeField] protected float attackRad = 2;

    protected override void Awake()
    {
        base.Awake();
        _animator = GetComponent<Animator>();
    }

    /*private void LateUpdate()
    {
        if (GetComponent<Animator>().enabled == true)
            transform.localEulerAngles = new Vector3(0, 0, currentZRot + transform.localEulerAngles.z);
        else
            transform.localEulerAngles = transform.localEulerAngles;
    }*/

    public Collider2D[] GetHitObjectInRange(Vector2 attackPoint, float _attackRad)
    {
        return Physics2D.OverlapCircleAll(attackPoint, _attackRad, _targetMask);
    }

    public override void ReceiveAttackMessage()
    {
        // Play animation
        base.ReceiveAttackMessage();
        if (!isLocal) return;
        // Detect enemies on range
        Collider2D[] hitObjects = GetHitObjectInRange(AttackPoint, attackRad);
        if (IsCritical()) OnCritical(hitObjects);
        else OnNormalAttack(hitObjects);
    }

    protected virtual void OnNormalAttack(Collider2D[] targets)
    {
        ModifyHpAll(targets, -baseAttack, Target.Monster);

        PlayAnimation();
        //currentZRot = transform.localEulerAngles.z;
    }

    protected virtual void OnCritical(Collider2D[] targets)
    {
        ModifyHpAll(targets, -baseAttack * 2, Target.Monster);

        PlayAnimation();
        //currentZRot = transform.localEulerAngles.z;
    }

    protected void ModifyHpAll(Collider2D[] targets, float amount, Target target)
    {
        foreach (Collider2D col in targets)
        {
            switch (target)
            {
                case Target.Player:
                    if (col.TryGetComponent(out Player player)) NetworkClient.Instance.ModifyHp(player, amount);
                    break;
                case Target.Statue:
                    if (col.TryGetComponent(out Statue _)) NetworkClient.Instance.ModifyHp(target, amount);
                    break;
                case Target.Wall:
                    if (col.TryGetComponent(out Wall wall)) NetworkClient.Instance.ModifyHp(wall, amount);
                    break;
                case Target.Monster:
                    //Debug.Log($"Monster {monster} get");
                    if (col.TryGetComponent(out Monster monster)) NetworkClient.Instance.ModifyHp(monster, amount);
                    break;
            }
            
        }
    }

    // Visually attack ---------------------------------------------------------------
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(AttackPoint, attackRad);
    }

    protected override void PlayAnimation()
    {
        base.PlayAnimation();
        
        if(_animator) _animator.Play("Attack");
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
