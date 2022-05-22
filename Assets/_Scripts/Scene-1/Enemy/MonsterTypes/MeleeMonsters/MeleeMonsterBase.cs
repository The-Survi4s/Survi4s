using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MeleeMonsterBase : Monster
{
    protected override void Attack(Component nearestObj)
    {
        MeleeAttack(nearestObj);
    }

    private void MeleeAttack(Component nearestObj)
    {
        if (!NetworkClient.Instance.isMaster) return;
        switch (nearestObj)
        {
            case Wall wall:
                OnAttackWall(wall);
                break;
            case Player player:
                OnAttackPlayer(player);
                break;
            case Statue _:
                OnAttackStatue();
                break;
        }
    }

    protected virtual void OnAttackStatue()
    {
        Debug.Log("Send damage statue!");
        NetworkClient.Instance.ModifyStatueHp(-currentStat.atk);
    }

    protected virtual void OnAttackPlayer(Player player)
    {
        var players = GetTargetPlayers();
        foreach (var p in players)
        {
            NetworkClient.Instance.ModifyPlayerHp(p.name, -currentStat.atk);
        }
    }

    protected virtual void OnAttackWall(Wall wall)
    {
        NetworkClient.Instance.ModifyWallHp(wall.id, -currentStat.atk);
    }
}
