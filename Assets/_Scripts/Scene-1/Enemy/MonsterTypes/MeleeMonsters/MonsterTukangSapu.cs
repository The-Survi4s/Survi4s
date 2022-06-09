using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterTukangSapu : MeleeMonsterBase
{
    protected override void OnAttackPlayer(Player player)
    {
        var players = GetPlayersInRadius(); 
        foreach (var p in players)
        {
            NetworkClient.Instance.ModifyHp(p, -currentStat.atk / 20);
        }
    }
}
