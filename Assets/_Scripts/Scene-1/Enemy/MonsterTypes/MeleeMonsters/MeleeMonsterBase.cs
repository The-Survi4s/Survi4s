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
        audioManager.PlayRandom();

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
        NetworkClient.Instance.ModifyHp(Target.Statue, -currentStat.atk);
    }

    protected virtual void OnAttackPlayer(Player player)
    {
        //Debug.Log(name + " attack " + player.name);
        var players = GetPlayersInRadius(); //Debug.Log("Player count : " + players.Count);
        foreach (var p in players)
        {
            NetworkClient.Instance.ModifyHp(p, -currentStat.atk);
        }
    }

    protected virtual void OnAttackWall(Wall wall)
    {
        NetworkClient.Instance.ModifyHp(wall, -currentStat.atk);
    }
}
