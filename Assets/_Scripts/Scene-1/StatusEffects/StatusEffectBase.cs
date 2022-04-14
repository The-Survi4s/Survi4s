using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatusEffect
{
    Stun,
    Slow,
    Bleed,
    Weaken
}

public class StatusEffectFactory : MonoBehaviour
{
    private static CooldownSystem _cooldownSystem;
    void Awake()
    {
        _cooldownSystem = FindObjectOfType<CooldownSystem>();
    }

    public static StatusEffectBase CreateNew(Monster owner, StatusEffect statusEffect, float duration, int strength = 1)
    {
        switch (statusEffect)
        {
            case StatusEffect.Stun:
                return new StunEffect(_cooldownSystem, owner, duration);
            case StatusEffect.Slow:
                return new SlowEffect(_cooldownSystem, owner, duration, strength);
            case StatusEffect.Bleed:
                return new BleedEffect(_cooldownSystem, owner, duration, strength);
            case StatusEffect.Weaken:
                return new WeakenEffect(_cooldownSystem, owner, duration, strength);
            default:
                throw new ArgumentOutOfRangeException(nameof(statusEffect), statusEffect, null);
        }
    }
}

public abstract class StatusEffectBase : IHasCooldown
{
    protected Monster owner;
    protected Stat originalStat;
    protected Stat modifiedStat;

    protected int strength;
    protected float duration;

    [SerializeField] protected StatusEffect statusEffectName;

    private readonly CooldownData _cooldownData;

    public float cooldownDuration => duration;
    public float remainingTime => _cooldownData.remainingTime;

    protected StatusEffectBase(CooldownSystem cooldownSystem, Monster owner, float duration, int strength)
    {
        this.owner = owner;
        originalStat = owner.currentStat;
        modifiedStat = originalStat;
        if (strength < 0) strength = 0;
        this.strength = strength;
        this.duration = duration;
        _cooldownData = cooldownSystem.PutOnCooldown(this);
    }

    public void UpdateStat(Stat updatedOriginalStat)
    {
        originalStat = updatedOriginalStat;
    }

    protected void StartApplyEffect()
    {
        while (!_cooldownData.isDone)
        {
            ApplyEffect();
        }
    }
    protected abstract void ApplyEffect();
    public Stat newStat => modifiedStat;
}