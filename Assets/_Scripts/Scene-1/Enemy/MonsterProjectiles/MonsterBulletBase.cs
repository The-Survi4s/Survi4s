using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonsterBulletBase : BulletBase
{
    protected Monster owner;
    protected override sealed void OnHit(Collider2D col)
    {
        if (owner && NetworkClient.Instance.isMaster)
        {
            //Debug.Log("Hit! " + col);
            if (col.TryGetComponent(out Player player)) NetworkClient.Instance.ModifyHp(player, -owner.currentStat.atk);
            else if(col.TryGetComponent(out Wall wall)) NetworkClient.Instance.ModifyHp(wall, -owner.currentStat.atk);
            else if(col.TryGetComponent(out Statue statue)) NetworkClient.Instance.ModifyHp(statue, -owner.currentStat.atk);
        }
        OnEndOfTrigger();
    }

    protected virtual void AttackSomething(Component component)
    {
        Debug.Log("Success? " + NetworkClient.Instance.ModifyHp(component, -owner.currentStat.atk) + 
            "Tile:" + (component as DestroyableTile));
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
