using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeMonsterBase : Monster
{
    protected override void Attack(Component nearestObj)
    {
        if (!NetworkClient.Instance.isMaster) return;
        switch (nearestObj)
        {
            case Wall wall:
                NetworkClient.Instance.ModifyWallHp(wall.id, -currentStat.atk);
                break;
            case Player _:
                var players = GetTargetPlayers();
                foreach (var player in players)
                {
                    NetworkClient.Instance.ModifyPlayerHp(player.name, -currentStat.atk);
                }

                break;
            case Statue _:
                Debug.Log("Send damage statue!");
                NetworkClient.Instance.ModifyStatueHp(-currentStat.atk);
                break;
        }
    }
}
