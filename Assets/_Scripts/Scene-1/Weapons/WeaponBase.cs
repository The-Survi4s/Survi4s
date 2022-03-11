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
            if (IsLocal())
            {
                RotateWeapon(owner.GetComponent<CharacterController>().localMousePos);
            }
            else
            {
                RotateWeapon(owner.GetComponent<CharacterController>().syncMousePos);
            }
        }
    }
    private void RotateWeapon(Vector3 target)
    {
        float AngleRad = Mathf.Atan2(target.y - transform.position.y, target.x - transform.position.x);
        float AngleDeg = (180 / Mathf.PI) * AngleRad;
        transform.rotation = Quaternion.Euler(0, 0, AngleDeg);
    }

    public void Attack()
    {
        // Check cooldown
        if (Time.time >= nextAttackTime)
        {
            // Send attack massage
            NetworkClient.Instance.Attack();

            // Cooldown
            nextAttackTime = Time.time + cooldownTime;
        }
    }
    public bool IsLocal()
    {
        return owner.GetComponent<CharacterController>().isLocal;
    }
    public bool IsCrit()
    {
        return false;
    }
    public Vector2 GetOwnerAttackPoint()
    {
        if(owner == null)
        {
            return new Vector2 (0, 0);
        }

        return owner.GetComponent<CharacterWeapon>().GetAttackPoint().position;
    }
    public Collider2D[] GetHitObjectInRange(Vector2 attackPoint, float attackRad, LayerMask tergetLayer)
    {
        return Physics2D.OverlapCircleAll(attackPoint, attackRad, tergetLayer);
    }

    public abstract void OnAttack();
    public abstract void SpawnBullet(Vector2 spawnPos, Vector2 mousePos);

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
    public void UnequipWeapon(CharacterWeapon player, Vector2 dropPos, float z)
    {
        if(player.gameObject.name == owner.name)
        {
            owner = null;
            transform.position = dropPos;
            transform.rotation = Quaternion.Euler(0, 0, z);
        }
    }
}
