using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponShield : WeaponMelee
{
    [SerializeField, Min(0)] private float _domeDiameneter = 5;
    [SerializeField, Min(0)] private float _domeDuration = 5;
    private bool isDomeActive => _domeEndTime > Time.time;
    private float _domeEndTime;
    protected override void Update()
    {
        base.Update();
        foreach (var col in GetHitObjectInRange(AttackPoint, attackRad + (isDomeActive ? _domeDiameneter : 0)))
        {
            if (col.TryGetComponent(out MonsterBulletBase monsterBullet))
            {
                NetworkClient.Instance.DestroyBullet(monsterBullet.id);
            }
        }
    }

    protected override void OnCritical(Collider2D[] targets)
    {
        base.OnCritical(targets);
        _domeEndTime = Time.time + _domeDuration;
    }
}
