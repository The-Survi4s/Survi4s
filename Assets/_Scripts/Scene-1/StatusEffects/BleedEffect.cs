using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BleedEffect : StatusEffectBase
{
    public BleedEffect(CooldownSystem cooldownSystem, Monster owner, float duration, int strength) : base(cooldownSystem, owner, duration, strength)
    {
        statusEffectName = StatusEffect.Bleed;
        StartApplyEffect();
    }

    protected override async void ApplyEffect()
    {
        owner.ModifyHitPoint(-strength, null);
        await Task.Delay(1000);
    }
}
