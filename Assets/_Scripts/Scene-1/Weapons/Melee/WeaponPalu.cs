using UnityEngine;

public class WeaponPalu : WeaponMelee
{
    [SerializeField] private LayerMask wallMask;

    protected override void OnNormalAttack(Collider2D[] targets)
    {
        foreach (Collider2D x in targets)
        {
            if (x.TryGetComponent(out Wall wall))
            {
                NetworkClient.Instance.ModifyWallHp(wall.id, 10);
                Debug.Log("We repair " + x.name);
            }
            else if(x.TryGetComponent(out Monster monster))
            {
                NetworkClient.Instance.ModifyMonsterHp(monster.id, -baseAttack);
            }
        }
    }

    protected override void OnCritical(Collider2D[] targets)
    {
        foreach (Collider2D x in targets)
        {
            if (x.TryGetComponent(out Wall wall))
            {
                NetworkClient.Instance.ModifyWallHp(wall.id, 20);
                Debug.Log("We super repair " + x.name);
            }
            else if (x.TryGetComponent(out Monster monster))
            {
                NetworkClient.Instance.ModifyMonsterHp(monster.id, -baseAttack);
                NetworkClient.Instance.ApplyStatusEffectToMonster(monster.id, StatusEffect.Stun, 1, 1);
            }
        }
    }
}
