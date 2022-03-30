using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTipeX : BulletBase
{
    [SerializeField] private const float StunDuration = 4f;

    protected override void OnCriticalShot(Monster monster)
    {
        base.OnCriticalShot(monster);
        NetworkClient.Instance.ApplyStatusEffectToMonster(monster.id, StatusEffect.Stun, 1, StunDuration);
    }
}
