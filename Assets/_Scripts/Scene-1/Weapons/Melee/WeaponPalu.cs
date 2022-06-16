using UnityEngine;

public class WeaponPalu : WeaponMelee
{
    [SerializeField] private int repairAmount = 5;
    protected override void OnNormalAttack(Collider2D[] targets)
    {
        if(!PaluHit(targets, -baseAttack, repairAmount))
        {
            nextAttackTime = Time.time + cooldownDuration / 5;
        }
    }

    protected override void OnCritical(Collider2D[] targets)
    {
        if(PaluHit(targets, -baseAttack * 2, repairAmount * 4, true))
        {
            nextAttackTime = Time.time + cooldownDuration / 5;
        }
    }

    private bool PaluHit(Collider2D[] targets, float damage, int repairAmount, bool addStun = false)
    {
        int targetHitCount = 0;
        foreach (Collider2D x in targets)
        {
            if (x.TryGetComponent(out Wall wall))
            {
                //Debug.Log("It's a wall! Repair! " + x.name);
                NetworkClient.Instance.ModifyHp(wall, repairAmount);
                targetHitCount++;
            }
            else if (x.TryGetComponent(out BrokenWall brokenWall))
            {
                //Debug.Log("It's a broken wall! SUPER rebuilt! " + x.name);
                NetworkClient.Instance.RebuildWall(brokenWall.id, repairAmount * 2);
                targetHitCount++;
            }
            else if (x.TryGetComponent(out Monster monster))
            {
                NetworkClient.Instance.ModifyHp(monster, damage);
                if(addStun) NetworkClient.Instance.ApplyStatusEffectToMonster(monster.id, StatusEffect.Stun, 5, 1);
                targetHitCount++;
            }
        }
        if (targetHitCount == 0) return false;
        else return true;
    }
}
