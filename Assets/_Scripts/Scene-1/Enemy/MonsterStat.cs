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
}

[Serializable]
public class MonsterStat : IHasCooldown
{
    private Stat _defaultStat;
    [SerializeField] private Stat _stat;
    private CooldownSystem _cooldownSystem;
    private CooldownData _cooldownData;
    public float cooldownDuration => _defaultStat.atkCd;
    public bool isAttackReady => _cooldownData.isDone;
    public float remainingAttackCooldown => _cooldownData.remainingTime;

    public event Action OnHpZero;

    // Getter n setters
    public int hitPoint
    {
        get => _stat.hp;
        set => _stat.hp = ValidateHp(value);
    }

    public float attack
    {
        get => _stat.atk;
        set => _stat.atk = value;
    }

    public float cooldown => _stat.atkCd;

    public float moveSpeed
    {
        get => _stat.movSpd;
        set => _stat.movSpd = value;
    }

    public float rotationSpeed => _stat.rotSpd;

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
    public MonsterStat(CooldownSystem cooldownSystem, Stat newStat)
    {
        _cooldownSystem = cooldownSystem;
        _defaultStat = newStat;
        _stat = _defaultStat;
        StartCooldown();
    }

    public void UpdateStat()
    {
        _stat.atkCd = _cooldownData.remainingTime;
    }

    // -------------------------

    public void StartCooldown()
    {
        _cooldownData = _cooldownSystem.PutOnCooldown(this);
    }

    public Stat getStat => _stat;
}
