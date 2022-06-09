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
                //Debug.Log("It's a wall! Repair! " + x.name);
                NetworkClient.Instance.ModifyHp(wall, 5);
            }
            else if (x.TryGetComponent(out BrokenWall brokenWall))
            {
                //Debug.Log("It's a broken wall! Rebuilt! " + x.name);
                NetworkClient.Instance.RebuildWall(brokenWall.id, 10);
            }
            else if(x.TryGetComponent(out Monster monster))
            {
                //Debug.Log("Palu damages " + monster + " by " + -baseAttack);
                NetworkClient.Instance.ModifyHp(monster, -baseAttack);
            }
        }
    }

    protected override void OnCritical(Collider2D[] targets)
    {
        foreach (Collider2D x in targets)
        {
            if (x.TryGetComponent(out Wall wall))
            {
                //Debug.Log("It's a wall! Repair! " + x.name);
                NetworkClient.Instance.ModifyHp(wall, 20);
            }
            else if (x.TryGetComponent(out BrokenWall brokenWall))
            {
                //Debug.Log("It's a broken wall! SUPER rebuilt! " + x.name);
                NetworkClient.Instance.RebuildWall(brokenWall.id, 50);
            }
            else if (x.TryGetComponent(out Monster monster))
            {
                NetworkClient.Instance.ModifyHp(monster, -baseAttack * 2);
                NetworkClient.Instance.ApplyStatusEffectToMonster(monster.id, StatusEffect.Stun, 1, 1);
            }
        }
    }
}
