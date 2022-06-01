using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [SerializeField] protected float defaultBaseAttack;
    [SerializeField] protected float defaultCritRate;
    [SerializeField] protected float maxCooldownTime;

    public float baseAttack { get; protected set; }
    public float critRate { get; protected set; }
    public float cooldownTime { get; protected set; }
    protected float nextAttackTime;
    public bool isReady => Time.time >= nextAttackTime;
    public GameObject owner { get; private set; }
    public bool IsUsed() => owner != null;

    [SerializeField] protected Vector3 offset;

    [SerializeField] private bool isFacingRight;
    [SerializeField] private float rotValZ;

    protected Animator _animator;

    // Particles -----------------------------------------


    // Cached components --------------------
    protected Player ownerPlayer;

    public bool isLocal => ownerPlayer.isLocal;

    // Upgrade Weapon -----------------------------------
    // Serializefiel just for debugging in inspector
    [SerializeField] private int _upgradeCost = 4;
    [SerializeField] private int _weaponLevel = 1;
    [SerializeField] private int _weaponExp = 0;
    
    // -------------------
    protected virtual void Init()
    {
        baseAttack = defaultBaseAttack;
        critRate = defaultCritRate;
        cooldownTime = maxCooldownTime;
        ownerPlayer = owner?.GetComponent<Player>();
    }

    private void Awake() => Init();
    private void Update()
    {
        // Check if this weapon is equipped ----------------------------------------
        if (owner == null) return;
        // Follow owner
        transform.position = owner.transform.position +
                             (ownerPlayer.isFacingLeft ? 
                             new Vector3(-offset.x, offset.y, offset.z) : 
                             new Vector3(offset.x, offset.y, offset.z));
        // Rotate weapon based on owner mouse pos
        RotateWeapon(isLocal
            ? ownerPlayer.movement.localMousePos
            : ownerPlayer.movement.syncMousePos);

        // Flip
        rotValZ = transform.eulerAngles.z;
        if (rotValZ > 90.0f && rotValZ < 270.0f && isFacingRight)
        {
            isFacingRight = false;
            transform.localScale -= new Vector3(0, 2, 0);
        }
        else if ((rotValZ < 90.0f || rotValZ > 270.0f) && !isFacingRight)
        {
            isFacingRight = true;
            transform.localScale += new Vector3(0, 2, 0);
        }
        // Savety
        if(transform.localScale.y < -1)
        {
            float gap = transform.localScale.y + 1.0f;
            transform.localScale -= new Vector3(0, gap, 0);
        }
        else if (transform.localScale.y > 1)
        {
            float gap = transform.localScale.y + 1.0f;
            transform.localScale -= new Vector3(0, gap, 0);
        }
    }

    // Network methods -----------------------------------
    public void SendAttackMessage()
    {
        // Check cooldown
        if (!isReady) return;
        // Send attack message
        NetworkClient.Instance.StartAttackAnimation();
        StartCooldown();
    }
    public void StartCooldown()
    {
        nextAttackTime = Time.time + cooldownTime;
    }
    

    // Animation methods ---------------------------------------
    protected virtual void PlayAnimation()
    {
    }
    protected virtual void SpawnParticle() { }
    private void RotateWeapon(Vector3 target)
    {
        var angle = Mathf.Atan2(target.y - transform.position.y, target.x - transform.position.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // Attack methods --------------------------------------------
    public bool IsCritical() => Random.Range(0f, 100f) < critRate;
    public Vector2 GetOwnerAttackPoint() =>
        owner == null
        ? Vector2.zero
        : (Vector2)owner.GetComponent<PlayerWeaponManager>().GetAttackPoint().position;

    public virtual void ReceiveAttackMessage() => PlayAnimation();

    // Equip / UnEquip -------------------------------------------------
    public void EquipWeapon(PlayerWeaponManager player)
    {
        if (owner != null) return;
        owner = player.gameObject;
        ownerPlayer = owner.GetComponent<Player>();
    }
    public void UnEquipWeapon(PlayerWeaponManager player, Vector2 dropPos, float zRotation)
    {
        if (player.name != owner.name) return;
        owner = null;
        transform.position = dropPos;
        transform.rotation = Quaternion.Euler(0, 0, zRotation);
        ownerPlayer = null;

        if(transform.localScale.y < 1)
        {
            transform.localScale -= new Vector3(0, transform.localScale.y * 2, 0);
        }
    }

    // Upgrade Weapon
    // Upgrade Level -----------------------------------------------------------
    public void UpgradeWeaponLevel(int exp)
    {
        _weaponExp += exp;

        while (_weaponExp >= _upgradeCost)
        {
            NetworkClient.Instance.UpgradeWeapon(this.name);
            _weaponExp -= _upgradeCost;
        }
    }
    public void WeaponLevelUp()
    {
        // Increase stats
        if (_weaponLevel % 3 == 0)
        {
            critRate += (critRate / 10.0f);
        }
        else if (_weaponLevel % 3 == 1)
        {
            cooldownTime -= (cooldownTime / 10.0f);
        }
        else if (_weaponLevel % 3 == 2)
        {
            baseAttack += (baseAttack / 20.0f);
        }

        // Increase cost to upgrade
        _upgradeCost += (4 / 100 * _upgradeCost) + 4;
        // Increase weapon level
        _weaponLevel++;
    }
}
