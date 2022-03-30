using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunEffect : StatusEffectBase
{
    public StunEffect(CooldownSystem cooldownSystem, Stat originalStat, float duration) : base(cooldownSystem, originalStat, 1, duration)
    {
        statusEffectName = StatusEffect.Stun;
        StartApplyEffect();
    }

    protected override void ApplyEffect()
    {
        modifiedStat.movSpd = 0;
    }
}
