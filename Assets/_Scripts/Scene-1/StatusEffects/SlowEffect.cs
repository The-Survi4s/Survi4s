using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowEffect : StatusEffectBase
{
    public SlowEffect(CooldownSystem cooldownSystem, Stat originalStat, int strength, float duration) : base(cooldownSystem, originalStat, strength, duration)
    {
        statusEffectName = StatusEffect.Slow;
        StartApplyEffect();
    }

    protected override void ApplyEffect()
    {
        modifiedStat.movSpd = originalStat.movSpd / (strength + 1);
        modifiedStat.atkCd = originalStat.atkCd * (strength + 1);
    }
}
