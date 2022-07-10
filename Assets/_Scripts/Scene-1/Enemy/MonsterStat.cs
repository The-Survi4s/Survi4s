using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A struct to store stats. 
/// <br/>Currently only used for <see cref="Monster"/>s
/// </summary>
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

/// <summary>
/// Wrapper for <see cref="Stat"/>
/// </summary>
[Serializable]
public class MonsterStat : IHasCooldown
{
    private Stat _defaultStat;

    [SerializeField] private Stat _rawStat;

    private readonly Monster _owner;

    public event Action OnHpZero;

    public MonsterStat(CooldownSystem cooldownSystem, Stat defaultStat, Monster owner)
    {
        _cooldownSystem = cooldownSystem;
        _cooldownData = new CooldownData(this);
        _defaultStat = defaultStat;
        _rawStat = _defaultStat;
        _owner = owner;
        StartCooldown();
    }

    // Getter n setters
    #region Stat getters and setters
    public int hitPoint
    {
        get => _rawStat.hp;
        set 
        {
            _rawStat.hp = ValidateHp(value); 
            if(_rawStat.hp <= 0) OnHpZero?.Invoke();
        }
    }

    public float attack
    {
        get => _rawStat.atk;
        set => _rawStat.atk = value;
    }

    /// <summary>
    /// The current attack cooldown of this <see cref="Monster"/>
    /// </summary>
    public float cooldown => _rawStat.atkCd;

    public float moveSpeed
    {
        get => _rawStat.movSpd;
        set => _rawStat.movSpd = value;
    }

    public float rotationSpeed => _rawStat.rotSpd;

    /// <summary>
    /// Stat after updated
    /// </summary>
    public Stat getRawStat => _rawStat;

    /// <summary>
    /// The default stat to reset to
    /// </summary>
    public Stat getDefaultStat => _defaultStat; 
    #endregion

    // Validation methods
    /// <summary>
    /// Clamps <paramref name="value"/> to 0 and <see cref="_defaultStat"/>.
    /// <br/>Invokes <see cref="OnHpZero"/> when <see cref="Stat.hp"/> is 0
    /// </summary>
    private int ValidateHp(int value)
    {
        if (value > _defaultStat.hp)
        {
            return _defaultStat.hp;
        }

        if (value > 0) return value;
        return 0;
    }

    #region Cooldown

    private CooldownSystem _cooldownSystem;
    private CooldownData _cooldownData;

    /// <summary>
    /// The max cooldown time
    /// </summary>
    public float cooldownDuration => _defaultStat.atkCd;

    public bool isAttackReady => _cooldownData.isDone;

    /// <summary>
    /// Call this in an Update() from somewhere else
    /// </summary>
    public void UpdateStatCooldown()
    {
        _rawStat.atkCd = _cooldownData.remainingTime;
    }

    /// <summary>
    /// Starts cooldown and stuns the monster for 1 sec
    /// </summary>
    public void StartCooldown()
    {
        _cooldownData = _cooldownSystem.PutOnCooldown(this);
        _owner.AddStatusEffect(StatusEffectFactory.CreateNew(_owner, StatusEffect.Stun, 1));
    }

    #endregion
}
