using UnityEngine;

public class WeaponPalu : WeaponMelee
{
    protected override void OnNormalAttack(Collider2D[] targets)
    {
        foreach (Collider2D x in targets)
        {
            if (x.TryGetComponent(out Wall wall))
            {
                if(!(wall.hp < wall.maxHp)) return;
                NetworkClient.Instance.ModifyWallHp(wall.id, 5);
            }
            else if (x.TryGetComponent(out BrokenWall brokenWall))
            {
                Debug.Log("It's a broken wall! Rebuilt! " + x.name);
                NetworkClient.Instance.RebuildWall(brokenWall.id, 10);
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
            }
            else if (x.TryGetComponent(out BrokenWall brokenWall))
            {
                Debug.Log("It's a broken wall! SUPER rebuilt! " + x.name);
                NetworkClient.Instance.RebuildWall(brokenWall.id, 50);
            }
            else if (x.TryGetComponent(out Monster monster))
            {
                NetworkClient.Instance.ModifyMonsterHp(monster.id, -baseAttack);
                NetworkClient.Instance.ApplyStatusEffectToMonster(monster.id, StatusEffect.Stun, 1, 1);
            }
        }
    }
}
