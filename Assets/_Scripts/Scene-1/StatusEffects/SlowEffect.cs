using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowEffect : StatusEffectBase
{
    public SlowEffect(CooldownSystem cooldownSystem, Monster owner, float duration, int strength) : base(cooldownSystem, owner, duration, strength)
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
