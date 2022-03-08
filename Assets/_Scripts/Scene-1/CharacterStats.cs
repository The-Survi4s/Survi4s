using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [SerializeField] private int DefaultHitPoint;
    [SerializeField] private int DefaultBaseAttack;

    public int hitPoint { get; private set; }
    public int baseAttack { get; private set; }

    private WeaponBase weapon;
    [SerializeField] private float weaponRadarRange;
    public bool weaponIsInRange { get; private set; }

    private void Start()
    {
        hitPoint = DefaultHitPoint;
        baseAttack = DefaultBaseAttack;
    }

    private void Update()
    {
        // Check Weapon in range
        CheckWeaponInRange();
    }

    public void EquipWeapon(string weaponName)
    {
        foreach(WeaponBase x in UnitManager.Instance.weapons)
        {
            if(x.name == weaponName)
            {
                // Unequip old weapon
                if(weapon != null)
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
        weapon.UnequipWeapon(this);
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
            if(dist <= weaponRadarRange && !x.isUsed())
            {
                weaponIsInRange = true;
                return;
            }
        }

        weaponIsInRange = false;
    }
}
