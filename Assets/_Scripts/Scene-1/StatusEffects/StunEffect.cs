using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunEffect : StatusEffectBase
{
    public StunEffect(CooldownSystem cooldownSystem, Monster owner, float duration) : base(cooldownSystem, owner, duration, 1)
    {
        statusEffectName = StatusEffect.Stun;
        StartApplyEffect();
    }

    protected override void ApplyEffect()
    {
        modifiedStat.movSpd = 0;
    }
}
