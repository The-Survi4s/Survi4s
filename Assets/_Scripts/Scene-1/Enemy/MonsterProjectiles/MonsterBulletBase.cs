using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonsterBulletBase : BulletBase
{
    protected Monster owner;
    protected override void OnHit(Collider2D col)
    {
        Player player = col.GetComponent<Player>();
        Wall wall = col.GetComponent<Wall>();
        Statue statue = col.GetComponent<Statue>();
        if (!player && !wall && !statue) return;
        if (owner && NetworkClient.Instance.isMaster)
        {
            if(player) AttackSomething(player);
            else if(wall) AttackSomething(wall);
            else if(statue) AttackSomething(statue);
        }
        OnEndOfTrigger();
    }

    protected virtual void AttackSomething(Component component)
    {
        switch (component)
        {
            case Player obj:
                NetworkClient.Instance.ModifyPlayerHp(obj.name, -owner.currentStat.atk);
                break;
            case Wall obj:
                NetworkClient.Instance.ModifyWallHp(obj.id, -owner.currentStat.atk);
                break;
            case Statue obj:
                NetworkClient.Instance.ModifyStatueHp(-owner.currentStat.atk);
                break;
        }
    }

    public void Init(Monster owner, Vector2 targetPos, int bulletId)
    {
        Debug.Log($"monster bullet {bulletId}: from {transform.position} to {targetPos}");
        this.owner = owner;
        Initialize(targetPos, bulletId);
    }
}
