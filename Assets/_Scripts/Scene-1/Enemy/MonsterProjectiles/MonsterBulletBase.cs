using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonsterBulletBase : BulletBase
{
    protected Monster owner;
    protected override sealed void OnHit(Collider2D col)
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
        NetworkClient.Instance.ModifyHp(component, -owner.currentStat.atk);
        /*
        switch (component)
        {
            case Player obj:
                //Debug.Log(name + " attack " + obj.name);
                NetworkClient.Instance.ModifyHp(Target.Player, obj.name, -owner.currentStat.atk);
                break;
            case Wall obj:
                NetworkClient.Instance.ModifyHp(Target.Wall, obj.id, -owner.currentStat.atk);
                break;
            case Statue _:
                NetworkClient.Instance.ModifyHp(Target.Statue, -owner.currentStat.atk);
                break;
        }
        */
    }

    public void Initialize(Monster owner, Vector2 targetPos, int bulletId)
    {
        //Debug.Log($"monster bullet {bulletId}: from {transform.position} to {targetPos}");
        this.owner = owner;
        Initialize(targetPos, bulletId);
    }
}
