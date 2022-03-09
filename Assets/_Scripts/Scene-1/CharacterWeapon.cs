using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterWeapon : MonoBehaviour
{
    private WeaponBase weapon;
    [SerializeField] private float weaponRadarRange;
    public bool weaponIsInRange { get; private set; }

    [SerializeField] private Transform attackPoint;

    private void Update()
    {
        // Check Weapon in range
        CheckWeaponInRange();
    }

    // Equip and unequip ---------------------------------------------------------------------
    public void EquipWeapon()
    {
        if (weaponIsInRange)
        {
            NetworkClient.Instance.EquipWeapon(GetClosestWeapon());
        }
    }
    public void OnEquipWeapon(string weaponName)
    {
        foreach (WeaponBase x in UnitManager.Instance.weapons)
        {
            if (x.name == weaponName)
            {
                // Unequip old weapon
                if (weapon != null)
                {
                    UnEquipWeapon();
                }

                // Equip new weapon
                x.EquipWeapon(this);
                weapon = x;
            }
        }
    }
    public void UnEquipWeapon()
    {
        weapon.UnequipWeapon(this, weapon.transform.position);
    }

    // Attack --------------------------------------------------------------------------------
    public void Attack()
    {
        if(weapon != null)
        {
            weapon.Attack();
        }
    }
    public void OnNormalAttack(Vector2 mousePos)
    {
        weapon.OnAttack(mousePos);
    }
    public void OnCritAttack(Vector2 mousePos)
    {
        weapon.OnCritical(mousePos);
    }
    public Transform GetAttackPoint()
    {
        return attackPoint;
    }

    // Find and get name of closest weapon ---------------------------------------------------
    public string GetClosestWeapon()
    {
        GameObject temp = null;
        float minDist = Mathf.Infinity;

        foreach (WeaponBase x in UnitManager.Instance.weapons)
        {
            float dist = Vector3.Distance(x.gameObject.transform.position, transform.position);
            if (dist < minDist && !x.isUsed())
            {
                temp = x.gameObject;
                minDist = dist;
            }
        }

        return temp.name;
    }
    // Check there's weapon in reange --------------------------------------------------------
    private void CheckWeaponInRange()
    {
        foreach (WeaponBase x in UnitManager.Instance.weapons)
        {
            Vector3 target = x.transform.position;
            target.z = transform.position.z;
            float dist = Vector3.Distance(target, transform.position);
            if (dist <= weaponRadarRange && !x.isUsed())
            {
                weaponIsInRange = true;
                return;
            }
        }

        weaponIsInRange = false;
    }
}
