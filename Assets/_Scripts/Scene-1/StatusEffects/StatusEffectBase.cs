using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatusEffect
{
    Stun,
    Slow,
    Bleed
}

public class StatusEffectFactory : MonoBehaviour
{
    private static CooldownSystem _cooldownSystem;
    void Awake()
    {
        _cooldownSystem = FindObjectOfType<CooldownSystem>();
    }
    public static StatusEffectBase Stun(Stat originalStat, float duration)
    {
        return new StunEffect(_cooldownSystem, originalStat, 4f);
    }

    public static StatusEffectBase CreateNew(Stat rawStat, StatusEffect statusEffect, int strength, float duration)
    {
        throw new System.NotImplementedException();
    }
}

public abstract class StatusEffectBase : IHasCooldown
{
    protected Stat originalStat;
    protected Stat modifiedStat;

    protected int strength;
    protected float duration;

    [SerializeField] protected StatusEffect statusEffectName;

    private readonly CooldownData _cooldownData;

    public float cooldownDuration => duration;

    protected StatusEffectBase(CooldownSystem cooldownSystem, Stat originalStat, int strength, float duration)
    {
        this.originalStat = originalStat;
        modifiedStat = originalStat;
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
    public Stat NewStat => modifiedStat;
}