using UnityEngine;

public class WeaponPalu : WeaponMelee
{
    protected override void OnNormalAttack(Collider2D[] targets)
    {
        Debug.Log("Palu normal attack");
        foreach (Collider2D x in targets)
        {
            Debug.Log($"collider x = {x}");
            if (x.TryGetComponent(out Wall wall))
            {
                Debug.Log("It's a wall! So we repair " + x.name);
                NetworkClient.Instance.ModifyWallHp(wall.id, 10);
            }
            else if(x.TryGetComponent(out Monster monster))
            {
                Debug.Log("It's a monster, so we smash " + x.name);
                NetworkClient.Instance.ModifyMonsterHp(monster.id, -baseAttack);
            }
        }
    }

    protected override void OnCritical(Collider2D[] targets)
    {
        Debug.Log("Palu crit attack");
        foreach (Collider2D x in targets)
        {
            if (x.TryGetComponent(out Wall wall))
            {
                Debug.Log("We super repair " + x.name);
                NetworkClient.Instance.ModifyWallHp(wall.id, 20);
            }
            else if (x.TryGetComponent(out Monster monster))
            {
                NetworkClient.Instance.ModifyMonsterHp(monster.id, -baseAttack);
                NetworkClient.Instance.ApplyStatusEffectToMonster(monster.id, StatusEffect.Stun, 1, 1);
            }
        }
    }
}
