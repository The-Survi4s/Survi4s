using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSticky : PlayerBulletBase
{
    [SerializeField] private const float Duration = 5f;

    protected override void OnCriticalShot(Monster monster)
    {
        base.OnCriticalShot(monster);
        NetworkClient.Instance.ApplyStatusEffectToMonster(monster.id, StatusEffect.Slow, Duration, 1);
    }
}
