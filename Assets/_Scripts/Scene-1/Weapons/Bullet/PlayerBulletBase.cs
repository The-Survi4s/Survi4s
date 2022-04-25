using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBulletBase : BulletBase
{
    [SerializeField] protected float criticalMultiplier = 1.5f;

    protected WeaponBase weapon { get; private set; }
    public bool isLocal { get; private set; }

    protected override void OnHit(Collider2D col)
    {
        Debug.Log("Hit a collider!");
        Monster monster = col.GetComponent<Monster>();
        if (!monster) return;
        Debug.Log($"it's a {monster}");
        if (weapon && isLocal)
        {
            Debug.Log("Attack!");
            if (!weapon.IsCritical())
            {
                OnNormalShot(monster);
            }
            else
            {
                OnCriticalShot(monster);
            }
            OnEndOfTrigger();
        }
    }

    protected virtual void OnNormalShot(Monster monster)
    {
        // Damage here
        NetworkClient.Instance.ModifyMonsterHp(monster.id, -weapon.baseAttack);
    }

    protected virtual void OnCriticalShot(Monster monster)
    {
        // More damage
        NetworkClient.Instance.ModifyMonsterHp(monster.id, -weapon.baseAttack * criticalMultiplier);
    }

    public void Init(WeaponBase weaponOrigin, Vector2 mousePos, int bulletId, bool isLocal = false)
    {
        float inAccuracy = 0;
        if (weaponOrigin is WeaponRange wr) inAccuracy = wr.inAccuracy;
        mousePos += new Vector2(Random.Range(-inAccuracy, inAccuracy), Random.Range(-inAccuracy, inAccuracy));
        weapon = weaponOrigin;
        this.isLocal = isLocal;
        Initialize(mousePos, bulletId);
    }
}
