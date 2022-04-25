using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletRock : PlayerBulletBase
{
    [SerializeField] private const float Duration = 3f;

    protected override void OnCriticalShot(Monster monster)
    {
        base.OnCriticalShot(monster);
        NetworkClient.Instance.ApplyStatusEffectToMonster(monster.id, StatusEffect.Stun, Duration, 1);
    }
}
