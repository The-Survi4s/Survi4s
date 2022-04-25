using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeakenEffect : StatusEffectBase
{
    public WeakenEffect(CooldownSystem cooldownSystem, Monster owner, float duration, int strength) : base(cooldownSystem, owner, duration, strength)
    {
        statusEffectName = StatusEffect.Stun;
        StartApplyEffect();
    }

    protected override void ApplyEffect()
    {
        modifiedStat.atk = originalStat.atk / (strength + 1);
    }
}
