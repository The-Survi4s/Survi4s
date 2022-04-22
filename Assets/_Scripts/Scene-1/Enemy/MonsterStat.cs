using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Stat
{
    public int hp;
    public float atk;
    public float atkCd;
    public float movSpd;
    public float rotSpd;
    public float acceleration;
}

[Serializable]
public class MonsterStat : IHasCooldown
{
    private Stat _defaultStat;
    [SerializeField] private Stat _rawStat;
    private CooldownSystem _cooldownSystem;
    private CooldownData _cooldownData;
    private readonly Monster _owner;
    public float cooldownDuration => _defaultStat.atkCd;
    public bool isAttackReady => _cooldownData.isDone;
    public float remainingAttackCooldown => _cooldownData.remainingTime;

    public event Action OnHpZero;

    // Getter n setters
    public int hitPoint
    {
        get => _rawStat.hp;
        set => _rawStat.hp = ValidateHp(value);
    }

    public float attack
    {
        get => _rawStat.atk;
        set => _rawStat.atk = value;
    }

    public float cooldown => _rawStat.atkCd;

    public float moveSpeed
    {
        get => _rawStat.movSpd;
        set => _rawStat.movSpd = value;
    }

    public float rotationSpeed => _rawStat.rotSpd;

    // Validation methods
    private int ValidateHp(int value)
    {
        if (value > _defaultStat.hp)
        {
            return _defaultStat.hp;
        }

        if (value > 0) return value;
        OnHpZero?.Invoke();
        return 0;
    }

    // ------------------------
    public MonsterStat(CooldownSystem cooldownSystem, Stat defaultStat, Monster owner)
    {
        _cooldownSystem = cooldownSystem;
        _cooldownData = new CooldownData(this);
        _defaultStat = defaultStat;
        _rawStat = _defaultStat;
        _owner = owner;
        StartCooldown();
    }

    public void UpdateStatCooldown()
    {
        _rawStat.atkCd = _cooldownData.remainingTime;
    }

    // -------------------------

    public void StartCooldown()
    {
        _cooldownData = _cooldownSystem.PutOnCooldown(this);
        _owner.AddStatusEffect(StatusEffectFactory.CreateNew(_owner, StatusEffect.Stun, 1));
    }

    public Stat getRawStat => _rawStat;
    public Stat getDefaultStat => _defaultStat;
}
