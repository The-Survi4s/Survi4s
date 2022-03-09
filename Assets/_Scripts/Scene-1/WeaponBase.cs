using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [SerializeField] private float DefaultBaseAttack;
    [SerializeField] private float DefaultCritRate;
    [SerializeField] private float DefaultCooldownTime;

    public float baseAttack { get; private set; }
    public float critRate { get; private set; }
    public float cooldownTime { get; private set; }
    private float nextAttackTime = 0f;

    public GameObject owner { get; private set; }
    [SerializeField] private Vector3 offset;

    private bool isFacingLeft;

    private void Start()
    {
        baseAttack = DefaultBaseAttack;
        critRate = DefaultCritRate;
        cooldownTime = DefaultCooldownTime;
    }
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

    public void Attack()
    {
        // Check cooldown
        if (Time.time >= nextAttackTime)
        {
            Vector2 mousePos = owner.GetComponent<CharacterController>().syncMousePos;
            // Calculate crit
            if (IsCrit())
            {
                NetworkClient.Instance.CritAttack(mousePos);
            }
            else
            {
                NetworkClient.Instance.NormalAttack(mousePos);
            }

            // Cooldown
            nextAttackTime = Time.time + cooldownTime;
        }
    }
    private bool IsCrit()
    {
        return false;
    }

    public abstract void OnAttack(Vector2 mousePos);
    public abstract void OnCritical(Vector2 mousePos);


    public bool isUsed()
    {
        if(owner != null)
        {
            return true;
        }
        return false;
    }

    public void EquipWeapon(CharacterWeapon player)
    {
        if(owner == null)
        {
            owner = player.gameObject;
        }
    }
    public void UnequipWeapon(CharacterWeapon player, Vector2 dropPos)
    {
        if(player.gameObject.name == owner.name)
        {
            owner = null;
            transform.position = dropPos;
        }
    }
}
