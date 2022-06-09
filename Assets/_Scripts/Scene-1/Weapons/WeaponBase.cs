using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [SerializeField] protected float defaultBaseAttack;
    [SerializeField] protected float defaultCritRate;
    [SerializeField] protected float maxCooldownTime;
    [SerializeField] protected float _attackDistance = 2;

    [field: SerializeField] public float baseAttack { get; protected set; }
    public float critRate { get; protected set; }
    public float cooldownTime { get; protected set; }
    protected float nextAttackTime;
    public bool isReady => Time.time >= nextAttackTime;
    protected Player ownerPlayer;
    public bool IsUsed() => ownerPlayer;

    [SerializeField] protected Vector3 offset;

    private bool isFacingRight;
    private float rotValZ;

    protected Animator _animator;
    protected SpriteRenderer _renderer;

    [SerializeField] protected GameObject _particleToSpawn;
    [SerializeField] protected float _particleLifetime = 2;

    [SerializeField] public Sprite uiSprite;

    public bool isLocal => ownerPlayer.isLocal;

    // Upgrade Weapon -----------------------------------
    // Serializefield just for debugging in inspector
    [field: SerializeField] public int UpgradeCost { get; private set; }
    [field: SerializeField] public int Level { get; private set; }
    
    // -------------------
    protected virtual void Init()
    {
        baseAttack = defaultBaseAttack;
        critRate = defaultCritRate;
        cooldownTime = maxCooldownTime;
        Level = 1;
    }

    protected virtual void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        Init();
    }

    protected virtual void Update()
    {
        // Check if this weapon is equipped ----------------------------------------
        if (ownerPlayer == null) return;
        // Follow owner
        transform.position = ownerPlayer.transform.position +
                             (ownerPlayer.isFacingLeft ? 
                             new Vector3(-offset.x, offset.y, offset.z) : 
                             new Vector3(offset.x, offset.y, offset.z));
        // Rotate weapon based on owner mouse pos
        RotateWeapon(isLocal
            ? ownerPlayer.movement.localMousePos
            : ownerPlayer.movement.syncMousePos);

        // Flip y
        rotValZ = transform.eulerAngles.z;
        /*
        if (rotValZ > 90.0f && rotValZ < 270.0f && isFacingRight)
        {
            isFacingRight = false;
            //transform.localScale -= new Vector3(0, 2, 0);
            _renderer.flipY = true;
        }
        else if ((rotValZ < 90.0f || rotValZ > 270.0f) && !isFacingRight)
        {
            isFacingRight = true;
            //transform.localScale += new Vector3(0, 2, 0);
            _renderer.flipY = false;
        }
        */
        _renderer.flipY = rotValZ > 90.0f && rotValZ < 270.0f;
        /*
        // Safety
        if(transform.localScale.y < -1 || transform.localScale.y > 1)
        {
            float gap = transform.localScale.y + 1.0f;
            transform.localScale -= new Vector3(0, gap, 0);
        }
        */
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

    protected virtual void SpawnParticle() 
    {
        if (_particleToSpawn)
        {
            var go = Instantiate(_particleToSpawn, transform.position, Quaternion.identity);
            Destroy(go, _particleLifetime);
        }
    }

    private void RotateWeapon(Vector3 target)
    {
        var angle = Mathf.Atan2(target.y - transform.position.y, target.x - transform.position.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // Attack methods --------------------------------------------
    public bool IsCritical() => Random.Range(0f, 100f) < critRate;

    protected Vector2 AttackPoint => transform.position + transform.right * _attackDistance;

    public virtual void ReceiveAttackMessage() => PlayAnimation();

    // Equip / UnEquip -------------------------------------------------
    public void EquipWeapon(PlayerWeaponManager player)
    {
        if (!ownerPlayer) ownerPlayer = player.GetComponent<Player>();
        GameUIManager.Instance.ChangeWeaponImage(uiSprite);
    }
    public void UnEquipWeapon(PlayerWeaponManager player, Vector2 dropPos, float zRotation)
    {
        if (player.name != ownerPlayer.name) return;
        transform.position = dropPos;
        transform.rotation = Quaternion.Euler(0, 0, zRotation);
        ownerPlayer = null;
        GameUIManager.Instance.ChangeWeaponImage(null);
        if(transform.localScale.y < 1)
        {
            transform.localScale -= new Vector3(0, transform.localScale.y * 2, 0);
        }
    }

    // Upgrade Weapon
    // Upgrade Level -----------------------------------------------------------
    public int UpgradeWeaponLevel(int incomingXp)
    {
        if (incomingXp >= UpgradeCost)
        {
            NetworkClient.Instance.UpgradeWeapon(name);
            incomingXp -= UpgradeCost;
        }
        return incomingXp;
    }

    public void WeaponLevelUp()
    {
        // Increase stats
        if (Level % 3 == 0)
        {
            critRate += (critRate / 10.0f);
        }
        else if (Level % 3 == 1)
        {
            cooldownTime -= (cooldownTime / 10.0f);
        }
        else if (Level % 3 == 2)
        {
            defaultBaseAttack += (defaultBaseAttack / 20.0f);
            baseAttack = defaultBaseAttack;
        }

        // Increase cost to upgrade
        UpgradeCost += (4 / 100 * UpgradeCost) + 4;
        // Increase weapon level
        Level++;
    }

    public float LevelUpPreview_Atk => (Level % 3 == 2) ? defaultBaseAttack * 1.05f : defaultBaseAttack;
    public float LevelUpPreview_Crit => (Level % 3 == 0) ? critRate * 1.1f : critRate;
    public float LevelUpPreview_Cooldown => (Level % 3 == 1) ? cooldownTime * 0.9f : cooldownTime;
}
