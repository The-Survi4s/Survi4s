using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [SerializeField] private float DefaultBaseAttack;
    [SerializeField] private float DefaultCoolDownTime;

    private GameObject owner;
    [SerializeField] private Vector3 offset;

    private bool isFacingLeft;

    private void Update()
    {
        // Check if this weapon is equipped ----------------------------------------
        if(owner != null)
        {
            // Follow owner
            if (owner.GetComponent<CharacterController>().isFacingLeft && !isFacingLeft)
            {
                offset.x = -offset.x;
                isFacingLeft = true;
            }
            else if (!owner.GetComponent<CharacterController>().isFacingLeft && isFacingLeft)
            {
                offset.x = -offset.x;
                isFacingLeft = false;
            }
            transform.position = owner.transform.position + offset;

            // Rotate weapon based on owner mouse pos
            Vector3 target = owner.GetComponent<CharacterController>().syncMousePos;
            float AngleRad = Mathf.Atan2(target.y - transform.position.y, target.x - transform.position.x);
            float AngleDeg = (180 / Mathf.PI) * AngleRad;
            transform.rotation = Quaternion.Euler(0, 0, AngleDeg);
        }
    }

    public abstract void Attack();

    public bool isUsed()
    {
        if(owner != null)
        {
            return true;
        }
        return false;
    }

    public void EquipWeapon(CharacterStats player)
    {
        if(owner == null)
        {
            owner = player.gameObject;
        }
    }
    public void UnequipWeapon(CharacterStats player)
    {
        if(player.gameObject.name == owner.name)
        {
            owner = null;
        }
    }
}
